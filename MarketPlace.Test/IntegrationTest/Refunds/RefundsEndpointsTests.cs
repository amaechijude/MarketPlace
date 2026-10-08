using System.Net;
using System.Net.Http.Json;
using MarketPlace.Test.IntegrationTest.SetUp;

namespace MarketPlace.Test.IntegrationTest.Refunds;

public sealed class RefundsEndpointsTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    private const string refundsUrl = $"{apiBaseUrlv1}/refunds";

    [Fact]
    public async Task RequestRefund_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await httpClient.PostAsJsonAsync(
            $"{refundsUrl}/request",
            new { OrderId = Guid.NewGuid(), Reason = "Defective product" }
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProcessRefund_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await httpClient.PostAsJsonAsync(
            $"{refundsUrl}/{Guid.NewGuid()}/process",
            new { Status = "Approved" }
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
