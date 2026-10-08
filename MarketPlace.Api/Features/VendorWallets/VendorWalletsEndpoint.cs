using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.VendorWallets.GetWalletBalance;
using MarketPlace.Api.Features.VendorWallets.RequestPayout;

namespace MarketPlace.Api.Features.VendorWallets;

public sealed class VendorWalletsEndpoint : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("vendor/wallet").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    GetWalletBalanceHandler handler,
                    System.Security.Claims.ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                {
                    var response = await handler.HandleAsync(user, cancellationToken);
                    return response.ToMinimalApiResult();
                }
            )
            .WithName("GetWalletBalance")
            .WithSummary("Get vendor wallet balance")
            .Produces<ApiResponse<WalletBalanceResponse>>();

        group
            .MapPost(
                "/payouts",
                async (
                    RequestPayoutRequest request,
                    RequestPayoutHandler handler,
                    System.Security.Claims.ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                {
                    var response = await handler.HandleAsync(request, user, cancellationToken);
                    return response.ToMinimalApiResult();
                }
            )
            .WithName("RequestPayout")
            .WithSummary("Request a payout from vendor wallet")
            .Produces<ApiResponse<RequestPayoutResponse>>();
    }
}
