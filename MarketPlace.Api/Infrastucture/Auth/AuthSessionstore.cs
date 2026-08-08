using MarketPlace.Api.Common.Extensions;
using Microsoft.Extensions.Caching.Hybrid;
using System.Security.Cryptography;
using System.Text;

namespace MarketPlace.Api.Infrastucture.Auth;

public sealed class AuthSessionstore(HybridCache hybridCache) : ISingletonMarker
{
    public async ValueTask<string> CreateAsync(
        AuthSessionRecord sessionRecord,
        TimeSpan ttl,
        CancellationToken cancellationToken
    )
    {
        string token = GenerateToken();

        await hybridCache.SetAsync(
            key: HashKey(token),
            value: sessionRecord,
            options: new HybridCacheEntryOptions
            {
                Expiration = ttl,
                LocalCacheExpiration = ttl / 2,
            },
            cancellationToken: cancellationToken
        );

        return token;
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

    public async ValueTask DeleteAsync(string token, CancellationToken cancellationToken) =>
        await hybridCache.RemoveAsync(key: HashKey(token), cancellationToken: cancellationToken);

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
