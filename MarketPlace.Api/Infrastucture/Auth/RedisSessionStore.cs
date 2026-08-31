using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StackExchange.Redis;

namespace MarketPlace.Api.Infrastucture.Auth;

public sealed class RedisSessionStore(
    IConnectionMultiplexer connectionMultiplexer,
    TimeProvider timeProvider
) : IAuthSessionStore
{
    private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();

    public async Task<(string accessToken, DateTimeOffset expiresOn)> CreateAsync(
        Guid userid,
        IEnumerable<string> roles,
        CancellationToken ct
    )
    {
        var token = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));

        var expiresOn = timeProvider.GetUtcNow().Add(CustomAuthSchemeOptions.DefaultTimeSpan);
        AuthSessionRecord sessionRecord = new(userid, roles, expiresOn);
        var json = JsonSerializer.Serialize(sessionRecord);

        await _redis.StringSetAsync(
            key: HashKey(token),
            value: json,
            expiry: CustomAuthSchemeOptions.DefaultTimeSpan
        );

        return (token, expiresOn);
    }

    public async Task<AuthSessionRecord?> GetSessionAsync(string token, CancellationToken ct)
    {
        var json = await _redis.StringGetAsync(key: HashKey(token));
        return json.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<AuthSessionRecord>(json.ToString());
    }

    public async Task<(string accessToken, DateTimeOffset expiresOn)> RefreshSessionAsync(
        string token,
        AuthSessionRecord record,
        CancellationToken ct
    )
    {
        await DeleteAsync(token, ct);
        return await CreateAsync(record.UserId, record.Roles, ct);
    }

    public async Task DeleteAsync(string token, CancellationToken ct) =>
        await _redis.KeyDeleteAsync(key: HashKey(token));

    private static string HashKey(string token)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(token), hash);
        return Convert.ToHexStringLower(hash);
    }
}
