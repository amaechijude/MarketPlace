using System.Security.Cryptography;
using System.Text;
using MarketPlace.Api.Common.Extensions;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Infrastucture.Auth;

public sealed class AuthSessionstore(HybridCache hybridCache, TimeProvider timeProvider)
    : ISingletonMarker
{
    public async ValueTask<(string accessToken, TimeSpan ttl)> CreateAsync(
        Guid userId,
        IEnumerable<string> roles,
        CancellationToken ct
    )
    {
        var token = GenerateToken();
        var ttl = TimeSpan.FromDays(7);

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

    public async ValueTask<AuthSessionRecord?> GetSessionAsync(
        string token,
        CancellationToken cancellationToken
    ) =>
        await hybridCache.GetOrCreateAsync(
            key: HashKey(token),
            factory: _ => ValueTask.FromResult<AuthSessionRecord?>(null),
            cancellationToken: cancellationToken
        );

    public async ValueTask DeleteAsync(string token, CancellationToken cancellationToken) =>
        await hybridCache.RemoveAsync(key: HashKey(token), cancellationToken: cancellationToken);

    private static string HashKey(string token)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(token), hash);
        return Convert.ToHexString(hash);
    }

    private static string GenerateToken() =>
        Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
}
