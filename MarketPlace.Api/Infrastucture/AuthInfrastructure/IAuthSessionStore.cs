namespace MarketPlace.Api.Infrastucture.AuthInfrastructure;

public interface IAuthSessionStore
{
    Task<(
        string accessToken,
        DateTimeOffset expiresOn,
        DateTimeOffset absoluteExpiresOn
    )> CreateAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        CancellationToken ct,
        bool rememberMe = false
    );
    Task<(string accessToken, DateTimeOffset expiresOn)> RefreshSessionAsync(
        string token,
        AuthSessionRecord session,
        CancellationToken ct
    );
    Task DeleteAllUserSessionsAsync(Guid userId, CancellationToken ct);
    Task DeleteAsync(string token, CancellationToken ct);
    Task<AuthSessionRecord?> GetSessionAsync(string? token, CancellationToken ct);
}
