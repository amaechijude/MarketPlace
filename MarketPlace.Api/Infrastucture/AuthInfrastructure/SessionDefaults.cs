namespace MarketPlace.Api.Infrastucture.AuthInfrastructure;

// Put this in your own namespace. `nameof(MarketPlace)` below assumes that
// symbol resolves in your project, as it did in the original.

public static class SessionDefaults
{
    // Sliding idle timeout. After any renewal the session is guaranteed to live
    // at least (IdleTimeout - RenewalInterval) = 25 more minutes of inactivity.
    public static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan RenewalInterval = TimeSpan.FromMinutes(5);

    // Hard cap. 7 days is long for a marketplace handling payments;
    // consider 8-24 hours and rely on remember-me for convenience.
    public static readonly TimeSpan AbsoluteLifetime = TimeSpan.FromDays(7);

    public static readonly TimeSpan RememberMeIdleTimeout = TimeSpan.FromDays(7);
    public static readonly TimeSpan RememberMeRenewalInterval = TimeSpan.FromHours(1);
    public static readonly TimeSpan RememberMeAbsoluteLifetime = TimeSpan.FromDays(30);

    // When exceeded, the oldest sessions of that user are revoked at login.
    public const int MaxSessionsPerUser = 10;
}
