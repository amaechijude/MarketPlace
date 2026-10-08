using System.Net;
using MarketPlace.Test.IntegrationTest.SetUp;

namespace MarketPlace.Test.IntegrationTest.Wishlists;

public sealed class WishlistsEndpointsTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    private const string WishlistsUrl = $"{apiBaseUrlv1}/wishlists";

    [Fact]
    public async Task GetMyWishlist_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await httpClient.GetAsync($"{WishlistsUrl}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddProductToWishlist_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await httpClient.PostAsync(
            $"{WishlistsUrl}/products/{Guid.NewGuid()}",
            null
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RemoveProductFromWishlist_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await httpClient.DeleteAsync($"{WishlistsUrl}/products/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
