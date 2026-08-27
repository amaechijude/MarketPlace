using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Infrastucture.Auth;

public sealed class HybridCacheSessionstore(HybridCache hybridCache, TimeProvider timeProvider)
    : IAuthSessionStore
{
    public async Task<(string accessToken, TimeSpan ttl)> CreateAsync(
        Guid userId,
        IEnumerable<string> roles,
        CancellationToken ct
    )
    {
        var token = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
        var ttl = AuthSessionOptions.DefaultTimeSpan;

        var sessionRecord = new AuthSessionRecord(userId, roles, timeProvider.GetUtcNow().Add(ttl));

        await hybridCache.SetAsync(
            key: HashKey(token),
            value: sessionRecord,
            options: new HybridCacheEntryOptions
            {
                Expiration = ttl,
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
            },
            cancellationToken: ct
        );

        return (token, ttl);
    }

    public async Task<AuthSessionRecord?> GetSessionAsync(
        string token,
        CancellationToken cancellationToken
    ) =>
        await hybridCache.GetOrCreateAsync(
            key: HashKey(token),
            factory: _ => ValueTask.FromResult<AuthSessionRecord?>(null),
            cancellationToken: cancellationToken
        );

    public async Task<(string accessToken, TimeSpan ttl)> RefreshSessionAsync(
        string token,
        AuthSessionRecord record,
        CancellationToken ct
    )
    {
        await DeleteAsync(token, ct);
        return await CreateAsync(record.UserId, record.Roles, ct);
    }

    public async Task DeleteAsync(string token, CancellationToken cancellationToken) =>
        await hybridCache.RemoveAsync(key: HashKey(token), cancellationToken: cancellationToken);

    private static string HashKey(string token)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(token), hash);
        return Convert.ToHexStringLower(hash);
    }
}
