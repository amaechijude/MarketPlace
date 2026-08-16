using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

namespace MarketPlace.Api.Infrastucture.Cache;

public static class CacheServiceCollection
{
    public static IServiceCollection AddCacheInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var redisUrl = configuration.GetConnectionString("Redis") ?? string.Empty;

        services
            .AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisUrl))
            .AddStackExchangeRedisCache(o => o.InstanceName = nameof(MarketPlace));

        services
            .AddOptions<RedisCacheOptions>()
            .Configure(
                (RedisCacheOptions options, IServiceProvider sp) =>
                {
                    options.ConnectionMultiplexerFactory = () =>
                        Task.FromResult(sp.GetRequiredService<IConnectionMultiplexer>());
                }
            );
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(2),
            };
        });
        return services;
    }
}
