using System.Security.Cryptography;
using System.Text;
using MarketPlace.Api.Common.Extensions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MarketPlace.Api.Infrastucture.OtpService;

public sealed class OtpEmailHandler(
    IConnectionMultiplexer mux,
    OtpEmailQueue queue,
    IOptions<OtpOptions> options
) : ISingletonMarker
{
    private const int CodeLength = 6;
    private const int MaxAttempts = 5;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);

    // Atomic: count the attempt, compare, consume on success, burn after too many failures.
    private static readonly LuaScript VerifyScript = LuaScript.Prepare(
        """
        local stored = redis.call('HGET', @key, 'h')
        if not stored then return 0 end
        local attempts = redis.call('HINCRBY', @key, 'a', 1)
        if stored == @hash then
            redis.call('DEL', @key)
            return 1
        end
        if attempts >= tonumber(@max) then
            redis.call('DEL', @key)
        end
        return 0
        """
    );

    private readonly IDatabase _redis = mux.GetDatabase();
    private readonly byte[] _pepper = Convert.FromBase64String(options.Value.Secret);

    public async Task GenerateAndEnqueueAsync(
        Guid userId,
        string email,
        OtpType type,
        CancellationToken cancellationToken
    )
    {
        var rawCode = RandomNumberGenerator.GetInt32(1_000_000).ToString("D6");
        var key = Key(type, userId);

        // Write value + TTL atomically; resets the attempt counter for the new code
        var tx = _redis.CreateTransaction();
        _ = tx.HashSetAsync(key, [new("h", HashOtp(rawCode, type, userId)), new("a", 0)]);
        _ = tx.KeyExpireAsync(key, Ttl);
        await tx.ExecuteAsync();

        try
        {
            await queue.EnqueueAsync(new OtpEmailRequest(email, type, rawCode), cancellationToken);
        }
        catch
        {
            await _redis.KeyDeleteAsync(key, CommandFlags.FireAndForget);
        }
    }

    public async Task<bool> ValidateOtpAsync(
        string rawCode,
        OtpType type,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        if (rawCode is null || rawCode.Length != CodeLength || !rawCode.All(char.IsAsciiDigit))
            return false;

        var result = await _redis
            .ScriptEvaluateAsync(
                VerifyScript,
                new
                {
                    key = (RedisKey)Key(type, userId),
                    hash = HashOtp(rawCode, type, userId),
                    max = MaxAttempts,
                }
            )
            .WaitAsync(cancellationToken);

        return (int)result == 1;
    }

    private string HashOtp(string rawCode, OtpType type, Guid userId) =>
        Convert.ToHexStringLower(
            HMACSHA256.HashData(_pepper, Encoding.UTF8.GetBytes($"{rawCode}:{type}:{userId}"))
        );

    private static string Key(OtpType type, Guid userId) => $"otp:{type}:{userId}";
}
