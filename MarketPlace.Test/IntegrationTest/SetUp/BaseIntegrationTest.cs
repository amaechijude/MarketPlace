using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MarketPlace.Test.IntegrationTest.SetUp;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    public readonly HttpClient httpClient;
    public readonly IServiceScope scope;

    public BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        httpClient = factory.CreateClient();
        scope = factory.Services.CreateScope();

        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
    }
}
