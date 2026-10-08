using System.Net;
using System.Net.Http.Json;
using MarketPlace.Test.IntegrationTest.SetUp;

namespace MarketPlace.Test.IntegrationTest.Wishlists;

public sealed class WishlistsEndpointsTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    private const string wishlistsUrl = $"{apiBaseUrlv1}/wishlists";

    [Fact]
    public async Task GetMyWishlist_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await httpClient.GetAsync($"{wishlistsUrl}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddProductToWishlist_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await httpClient.PostAsync(
            $"{wishlistsUrl}/products/{Guid.NewGuid()}",
            null
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RemoveProductFromWishlist_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await httpClient.DeleteAsync($"{wishlistsUrl}/products/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
