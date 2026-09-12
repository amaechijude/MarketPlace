using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace MarketPlace.Test.SetUp;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder(
        "postgres:18-alpine"
    )
        .WithDatabase("marketplace")
        .WithUsername("marketplace")
        .WithPassword("marketplace")
        .Build();
    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:8.6").Build();

    public Task InitializeAsync() =>
        Task.WhenAll(_postgresContainer.StartAsync(), _redisContainer.StartAsync());

    // var options = new DbContextOptionsBuilder<AppDbContext>()
    //     .UseNpgsql(_postgresContainer.GetConnectionString())
    //     .Options;

    // await using var dbContext = new AppDbContext(options);
    // await dbContext.Database.MigrateAsync();


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(async services =>
        {
            var serviceDescriptorsToReplace = services
                .Where(s =>
                    s.ServiceType == typeof(AppDbContext)
                    || s.ServiceType == typeof(IConnectionMultiplexer)
                )
                .ToList();

            foreach (var descriptor in serviceDescriptorsToReplace)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(s =>
                s.UseNpgsql(
                    _postgresContainer.GetConnectionString(),
                    npgsqlOptionsAction =>
                    {
                        npgsqlOptionsAction.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null
                        );
                    }
                )
            );
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString())
            );
        });
    }

    Task IAsyncLifetime.DisposeAsync() =>
        Task.WhenAll(_postgresContainer.StopAsync(), _redisContainer.StopAsync());
}
