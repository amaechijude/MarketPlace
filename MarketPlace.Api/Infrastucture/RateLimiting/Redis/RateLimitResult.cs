namespace MarketPlace.Api.Infrastucture.RateLimiting.Redis;

public sealed record RateLimitResult(bool Allowed, double Remaining, long RetryAfter);
