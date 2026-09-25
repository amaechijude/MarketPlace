using System.Security.Cryptography;
using System.Text;

namespace MarketPlace.Api.Infrastucture.AuthInfrastructure;

public abstract class BaseAuthSessionStore : IAuthSessionStore
{
    public static string UserSessionsKey(Guid userId) => $"user-sessions:{userId}";

    public static string GenerateToken() =>
        Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));

    public static string HashKey(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(token), hash);
        return Convert.ToHexStringLower(hash);
    }

    public abstract Task<(string accessToken, DateTimeOffset expiresOn)> CreateAsync(
        Guid userId,
        IEnumerable<string> roles,
        CancellationToken ct
    );

    public abstract Task DeleteAsync(string token, CancellationToken cancellationToken);

    public abstract Task DeleteAllUserSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken
    );

    public abstract Task<AuthSessionRecord?> GetSessionAsync(
        string token,
        CancellationToken cancellationToken
    );
    public abstract Task<(string accessToken, DateTimeOffset expiresOn)> RefreshSessionAsync(
        string token,
        AuthSessionRecord record,
        CancellationToken ct
    );
}
