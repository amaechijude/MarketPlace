using System.Net;
using MarketPlace.Test.IntegrationTest.SetUp;

namespace MarketPlace.Test.IntegrationTest.Vendors;

public sealed class VendorEndpointsTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    private const string VendorUrl = $"{apiBaseUrlv1}/vendors";

    [Fact]
    public async Task ListVendors_ReturnsSuccess()
    {
        var response = await httpClient.GetAsync($"{VendorUrl}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetVendorProfile_WithInvalidId_ReturnsNotFound()
    {
        var response = await httpClient.GetAsync($"{VendorUrl}/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetVendorStore_WithInvalidSlug_ReturnsNotFound()
    {
        var response = await httpClient.GetAsync($"{VendorUrl}/store/invalid-store-slug");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
