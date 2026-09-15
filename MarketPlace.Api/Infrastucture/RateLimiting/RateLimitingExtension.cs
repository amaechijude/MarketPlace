using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Infrastucture.RateLimiting.Redis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;

namespace MarketPlace.Api.Infrastucture.RateLimiting;

public static class RateLimitingExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddRateLimitingInfrastructure(IConfiguration configuration)
        {
            services.AddCustomOptions(configuration)
                .AddCustomRedisRateLimiter()
                .AddRateLimiter(options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                    // return Api error response when request is rejected
                    options.OnRejected = async (context, _) =>
                    {
                        await Results
                            .Problem(statusCode: StatusCodes.Status429TooManyRequests)
                            .ExecuteAsync(context.HttpContext);
                    };

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

        private IServiceCollection AddCustomOptions(IConfiguration configuration)
        {
            services.AddOptions<IpAddresTokenBucketOptions>().Bind(configuration.GetSection(nameof(IpAddresTokenBucketOptions))).ValidateDataAnnotations().ValidateOnStart();
            services.AddOptions<EmailAddresTokenBucketOptions>().Bind(configuration.GetSection(nameof(EmailAddresTokenBucketOptions))).ValidateDataAnnotations().ValidateOnStart();
            return services;
        }

        private IServiceCollection AddCustomRedisRateLimiter()
        {
            services.AddKeyedSingleton<ITokenBucketLimiter, TokenBucketLimiter>(
              IpAddresTokenBucketOptions.Key,
              (sp, _) =>
              {
                  var option = sp.GetRequiredService<IOptions<IpAddresTokenBucketOptions>>().Value;

                  return new TokenBucketLimiter(sp.GetRequiredService<IConnectionMultiplexer>(), option.Capacity, option.RefillRate, option.RefillIntervalSeconds);
              }
          );

            services.AddKeyedSingleton<ITokenBucketLimiter, TokenBucketLimiter>(EmailAddresTokenBucketOptions.Key
                ,
                (sp, _) =>
                {
                    var option = sp.GetRequiredService<IOptions<EmailAddresTokenBucketOptions>>().Value;

                    return new TokenBucketLimiter(sp.GetRequiredService<IConnectionMultiplexer>(), option.Capacity, option.RefillRate, option.RefillIntervalSeconds);
                }
            );
            return services;
        }
    }
}

public sealed class IpAddresTokenBucketOptions
{
    [Range(4, 20)]
    public int Capacity { get; set; } = 15;

    [Range(1, 3)]
    public int RefillRate { get; set; } = 1;

    [Range(20, 120)]
    public int RefillIntervalSeconds { get; set; } = 60;

    public const string Key = "ip";
}

public sealed class EmailAddresTokenBucketOptions
{

    [Range(4, 20)]
    public int Capacity { get; set; } = 5;
    [Range(1, 3)]
    public int RefillRate { get; set; } = 1;
    [Range(20, 120)]
    public int RefillIntervalSeconds { get; set; } = 120;

    public const string Key = "email";
}
