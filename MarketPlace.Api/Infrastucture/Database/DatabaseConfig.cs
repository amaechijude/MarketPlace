using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Infrastucture.Database;

public static class DatabaseConfig
{
    public static IServiceCollection AddDatabaseInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        DotNetEnv.Env.TraversePath().Load();
        services
            .AddOptions<DatabaseOptions>()
            .Configure(options =>
                options.ConnectionString =
                    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                    ?? string.Empty
            )
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContextFactory<AppDbContext>(
            (sp, db) =>
            {
                //db.UseNpgsql("connectionString", options =>
                //{
                //    options.EnableRetryOnFailure(
                //        maxRetryCount: 5,
                //        maxRetryDelay: TimeSpan.FromSeconds(30),
                //        errorCodesToAdd: null
                //       );
                //});

                db.UseInMemoryDatabase(Guid.NewGuid().ToString());
            }
        );
        return services;
    }
}

public sealed class DatabaseOptions
{
    [Required, MinLength(5)]
    public string ConnectionString { get; set; } = string.Empty;
}
