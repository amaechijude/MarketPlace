using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MarketPlace.Api.Common.Extensions;
using StackExchange.Redis;

namespace MarketPlace.Api.Infrastucture.Auth;

public sealed class RedisSessionStore(
    IConnectionMultiplexer connectionMultiplexer,
    TimeProvider timeProvider
) : ISingletonMarker
{
    private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();

    public async Task<string> CreateAsynce(Guid userid, IEnumerable<string> roles)
    {
        var token = GenerateToken();

        var ttl = TimeSpan.FromDays(7);
        AuthSessionRecord sessionRecord = new(userid, roles, timeProvider.GetUtcNow().Add(ttl));
        var json = JsonSerializer.Serialize(sessionRecord);

        await _redis.StringSetAsync(key: HashKey(token), value: json, expiry: ttl);

        return token;
    }

    public async Task<AuthSessionRecord?> GetSessionAsync(string token)
    {
        var json = await _redis.StringGetAsync(key: HashKey(token));
        return json.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<AuthSessionRecord>(json.ToString());
    }

    public async Task DeleteAsync(string token) => await _redis.KeyDeleteAsync(key: HashKey(token));

    public async Task RefreshAsync(string token, TimeSpan ttl) =>
        await _redis.KeyExpireAsync(key: HashKey(token), expiry: ttl);

    private static string HashKey(string token)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(token), hash);
        return Convert.ToHexString(hash);
    }

    private static string GenerateToken() =>
        Convert
            .ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "_")
            .Replace("/", "_")
            .TrimEnd('=');
}
