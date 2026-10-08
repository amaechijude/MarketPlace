using System.Net;
using System.Net.Http.Json;
using MarketPlace.Test.IntegrationTest.SetUp;

namespace MarketPlace.Test.IntegrationTest.VendorWallets;

public sealed class VendorWalletsEndpointsTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    private const string VendorWalletsUrl = $"{apiBaseUrlv1}/vendor/wallet";

    [Fact]
    public async Task GetWalletBalance_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await httpClient.GetAsync($"{VendorWalletsUrl}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RequestPayout_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await httpClient.PostAsJsonAsync(
            $"{VendorWalletsUrl}/payouts",
            new
            {
                AmountInKobo = 1000,
                BankAccountName = "Test",
                BankAccountNumber = "1234567890",
                BankName = "Test Bank",
            }
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
