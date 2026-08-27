namespace MarketPlace.Api.Infrastucture.Auth;

public interface IAuthSessionStore
{
    Task<(string accessToken, TimeSpan ttl)> CreateAsync(
        Guid userId,
        IEnumerable<string> roles,
        CancellationToken ct
    );
    Task DeleteAsync(string token, CancellationToken cancellationToken);
    Task<AuthSessionRecord?> GetSessionAsync(string token, CancellationToken cancellationToken);

    Task<(string accessToken, TimeSpan ttl)> RefreshSessionAsync(
        string token,
        AuthSessionRecord record,
        CancellationToken ct
    );
}
