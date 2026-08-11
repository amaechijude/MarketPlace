using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
            .Validate(v => ValidatePostgresqlConnString(v.ConnectionString))
            .ValidateOnStart();

        services.AddDbContextFactory<AppDbContext>(
            (sp, db) =>
            {
                var dbValue = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                db.UseNpgsql(
                    dbValue.ConnectionString,
                    options =>
                    {
                        options.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null
                        );
                    }
                );
            }
        );
        return services;
    }

    private static bool ValidatePostgresqlConnString(string connectionString) =>
        connectionString.CharCountIsGreaterThanOrEqual('=', 5)
        && connectionString.CharCountIsGreaterThanOrEqual(';', 4)
        && connectionString.Contains("Host", StringComparison.OrdinalIgnoreCase);
}

public sealed class DatabaseOptions
{
    [Required, MinLength(5)]
    public string ConnectionString { get; set; } = string.Empty;
}
