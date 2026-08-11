using System.Security.Claims;
using System.Threading.RateLimiting;

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

            // news letter
            options.AddPolicy(
                policyName: RateLimitPolicyKeys.NewsLetterFixed,
                context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientIp(context),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 4,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        }
                    )
            );
        });

        return services;
    }

    private static string GetUserIdentifier(HttpContext context)
    {
        var user = context.User.FindFirst(ClaimTypes.NameIdentifier);

        return user is null ? "anonymous" : user.Value;
    }

    private static string GetClientIp(HttpContext context)
    {
        // cloud flare
        string? cloudflareIp = context.Request.Headers["cf-connecting-ip"];
        if (!string.IsNullOrWhiteSpace(cloudflareIp))
            return cloudflareIp;

        // nginx
        string? nginxIp = context.Request.Headers["x-real-ip"];
        if (!string.IsNullOrWhiteSpace(nginxIp))
            return nginxIp;

        // x forwaderd
        string? forwarded = context.Request.Headers["x-forwarded-for"];
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            var forwardedIps = forwarded
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();
            if (forwardedIps.Length > 0)
                return forwardedIps[^1];
        }

        // fall back
        var remoteIp = context.Connection.RemoteIpAddress;
        return remoteIp is not null ? remoteIp.ToString() : "anonymous";
    }
}
