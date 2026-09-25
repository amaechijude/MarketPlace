namespace MarketPlace.Api.Infrastucture.RateLimiting.Redis;

public interface ITokenBucketLimiter
{
    Task<RateLimitResult> AllowAsync(string key, CancellationToken cancellationToken);
}
