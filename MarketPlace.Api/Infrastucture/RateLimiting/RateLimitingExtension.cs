using System.Security.Claims;
using System.Threading.RateLimiting;
using RedisRateLimiting;
using StackExchange.Redis;

namespace MarketPlace.Api.Infrastucture.RateLimiting;

public static class RateLimitingExtension
{
    public static IServiceCollection AddRateLimitingInfrastructure(this IServiceCollection services)
    {
        var connectionMultplexer = services
            .BuildServiceProvider()
            .GetRequiredService<IConnectionMultiplexer>();
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

            // login policy
            options.AddPolicy(
                policyName: RateLimitPolicyKeys.LoginTokenBucket,
                context =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        // ip address as key
                        partitionKey: GetClientIp(context),
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 5,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            AutoReplenishment = true,
                            TokensPerPeriod = 1,
                            QueueLimit = 0,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        }
                    )
            );
            // redis

            // redis login policy
            options.AddPolicy(
                policyName: RateLimitPolicyKeys.RedisLoginTokenBucket,
                context =>
                    RedisRateLimitPartition.GetTokenBucketRateLimiter(
                        partitionKey: GetClientIp(context),
                        factory: _ => new RedisTokenBucketRateLimiterOptions
                        {
                            ConnectionMultiplexerFactory = () => connectionMultplexer,
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
                    RateLimitPartition.GetTokenBucketLimiter(
                        // ip address as key
                        partitionKey: GetClientIp(context),
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 5,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            AutoReplenishment = true,
                            TokensPerPeriod = 1,
                            QueueLimit = 0,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        }
                    )
            );

            // address
            options.AddPolicy(
                policyName: RateLimitPolicyKeys.AddressTokenBucket,
                context =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: GetUserIdentifier(context),
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

    private static string GetClientIp(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }

    private static string GetUserIdentifier(HttpContext context)
    {
        var user = context.User.FindFirst(ClaimTypes.NameIdentifier);

        return user is null ? "anonymous" : user.Value;
    }
}
