using System.Security.Claims;
using System.Threading.RateLimiting;
using MarketPlace.Api.Common.Extensions;
using RedisRateLimiting;
using StackExchange.Redis;

namespace MarketPlace.Api.Infrastucture.RateLimiting;

public static class RateLimitingExtension
{
    public static IServiceCollection AddRateLimitingInfrastructure(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            // return Api error response when request is rejected
            options.OnRejected = async (context, _) =>
            {
                await Results
                    .Problem(statusCode: StatusCodes.Status429TooManyRequests)
                    .ExecuteAsync(context.HttpContext);
            };
            // redis

            // redis login policy
            options.AddPolicy(
                policyName: RateLimitPolicyKeys.LoginTokenBucket,
                context =>
                    RedisRateLimitPartition.GetTokenBucketRateLimiter(
                        partitionKey: GetClientIp("login", context),
                        factory: _ => new RedisTokenBucketRateLimiterOptions
                        {
                            ConnectionMultiplexerFactory = () =>
                                context.RequestServices.GetRequiredService<IConnectionMultiplexer>(),
                            TokenLimit = 5,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            TokensPerPeriod = 1,
                        }
                    )
            );
            // redis
            // Register policy
            options.AddPolicy(
                policyName: RateLimitPolicyKeys.RegisterTokenBucket,
                context =>
                    RedisRateLimitPartition.GetTokenBucketRateLimiter(
                        // ip address as key
                        partitionKey: GetClientIp("register", context),
                        factory: _ => new RedisTokenBucketRateLimiterOptions
                        {
                            ConnectionMultiplexerFactory = () =>
                                context.RequestServices.GetRequiredService<IConnectionMultiplexer>(),
                            TokenLimit = 5,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            TokensPerPeriod = 1,
                        }
                    )
            );

            // address
            options.AddPolicy(
                policyName: RateLimitPolicyKeys.AddressTokenBucket,
                context =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: context.User.UserId,
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 6,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            AutoReplenishment = true,
                            TokensPerPeriod = 2,
                            QueueLimit = 0,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        }
                    )
            );
        });

        return services;
    }

    private static string GetClientIp(string prefix, HttpContext context) =>
        $"{prefix}:{context.Connection.RemoteIpAddress?.ToString()}" ?? "anonymous";
}
