using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace MarketPlace.Test.SetUp;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptors = services
                .Where(s =>
                    s.ServiceType == typeof(AppDbContext)
                    || (
                        s.ServiceType.FullName != null
                        && s.ServiceType.FullName.Contains("EntityFrameworkCore")
                    )
                )
                .ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            // swap with in memory db
            services.AddDbContext<AppDbContext>(s => s.UseInMemoryDatabase(databaseName: "test"));
        });
    }
}

// public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
// {
//     private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder(
//         "postgres:18-alpine"
//     )
//         .WithDatabase("test")
//         .WithUsername("admin")
//         .WithPassword("test")
//         .Build();
//     private readonly RedisContainer _redisContainer = new RedisBuilder("redis:8.6").Build();

//     public async Task InitializeAsync()
//     {
//         await _postgresContainer.StartAsync();
//         await _redisContainer.StartAsync();
//     }

//     // note the "new" keyword — WebApplicationFactory already exposes
//     // a DisposeAsync() that returns ValueTask; this hides it so xUnit's
//     // IAsyncLifetime.DisposeAsync() (Task) is the one invoked.
//     public new async Task DisposeAsync()
//     {
//         await _postgresContainer.DisposeAsync();
//     }

//     protected override void ConfigureWebHost(IWebHostBuilder builder)
//     {
//         builder.UseSetting("ConnectionStrings:Database", _postgresContainer.GetConnectionString());
//         builder.UseSetting("ConnectionStrings:Redis", _redisContainer.GetConnectionString());

//         Environment.SetEnvironmentVariable(
//             "ConnectionStrings:Database",
//             _postgresContainer.GetConnectionString()
//         );
//         Environment.SetEnvironmentVariable(
//             "ConnectionStrings:Redis",
//             _redisContainer.GetConnectionString()
//         );
//     }
// }
