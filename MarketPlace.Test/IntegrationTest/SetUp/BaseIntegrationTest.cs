using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MarketPlace.Test.IntegrationTest.SetUp;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    public const string apiBaseUrlv1 = "api/v1";
    public readonly HttpClient httpClient;
    public readonly IServiceScope scope;
    public readonly AppDbContext appDbContext;

    public BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        httpClient = factory.CreateClient();
        scope = factory.Services.CreateScope();
        appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        appDbContext.Database.Migrate();
    }
}
