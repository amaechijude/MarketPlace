namespace MarketPlace.Api.Infrastucture.RateLimiting.Redis;

/// <summary>
/// Result of a rate limit check.
/// </summary>
/// <param name="Allowed">Whether the request is allowed.</param>
/// <param name="Remaining">Number of tokens remaining in the bucket.</param>
public record RateLimitResult(bool Allowed, double Remaining);
