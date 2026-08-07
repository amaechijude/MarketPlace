using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MarketPlace.Test.IntegrationTest.SetupTests;

public sealed class CustomWebApplicationFactory :WebApplicationFactory<Program>
{

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbcontext = services
                .Where(d => d.ServiceType == typeof(AppDbContext) || d.ServiceType.FullName!.Contains("EntityFrameworkCore")).ToList();

         if (dbcontext.Count != 0)
            {
                foreach (var ct in dbcontext)
                {
                    services.Remove(ct);
                }
            }

            // swap with in memory db
            services.AddDbContext<AppDbContext>(s =>
                s.UseInMemoryDatabase(databaseName: "test"));
        }
    );
    }
   
}
