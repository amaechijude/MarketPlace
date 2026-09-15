using System.Net;

namespace MarketPlace.Api.Infrastucture.RateLimiting.Redis;

public interface ITokenBucketLimiter
{
    Task<RateLimitResult> AllowAsync(IPAddress? iPAddress);
    Task<RateLimitResult> AllowAsync(string key);
}
