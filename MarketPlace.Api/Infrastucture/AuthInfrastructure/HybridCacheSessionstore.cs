using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Infrastucture.AuthInfrastructure;

public sealed class HybridCacheSessionstore(HybridCache hybridCache, TimeProvider timeProvider)
    : BaseAuthSessionStore
{
    public override async Task<(string accessToken, DateTimeOffset expiresOn)> CreateAsync(
        Guid userId,
        IEnumerable<string> roles,
        CancellationToken ct
    )
    {
        var token = GenerateToken();
        var expiresOn = timeProvider.GetUtcNow().Add(CustomAuthSchemeOptions.DefaultTimeSpan);

        var sessionRecord = new AuthSessionRecord(userId, roles, expiresOn);

        await hybridCache.SetAsync(
            key: HashKey(token),
            value: sessionRecord,
            options: new HybridCacheEntryOptions
            {
                Expiration = CustomAuthSchemeOptions.DefaultTimeSpan,
                LocalCacheExpiration = TimeSpan.FromMinutes(2),
            },
            tags: [UserSessionsKey(userId)],
            cancellationToken: ct
        );

        return (token, expiresOn);
    }

    public override async Task<AuthSessionRecord?> GetSessionAsync(
        string token,
        CancellationToken cancellationToken
    ) =>
        await hybridCache.GetOrCreateAsync(
            key: HashKey(token),
            factory: _ => ValueTask.FromResult<AuthSessionRecord?>(null),
            cancellationToken: cancellationToken
        );

    public override async Task<(string accessToken, DateTimeOffset expiresOn)> RefreshSessionAsync(
        string token,
        AuthSessionRecord record,
        CancellationToken ct
    )
    {
        var result = await CreateAsync(record.UserId, record.Roles, ct);
        await DeleteAsync(token, ct);
        return result;
    }

    public override async Task DeleteAsync(string token, CancellationToken cancellationToken) =>
        await hybridCache.RemoveAsync(key: HashKey(token), cancellationToken: cancellationToken);

    public override async Task DeleteAllUserSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken
    ) => await hybridCache.RemoveByTagAsync(UserSessionsKey(userId), cancellationToken);
}
