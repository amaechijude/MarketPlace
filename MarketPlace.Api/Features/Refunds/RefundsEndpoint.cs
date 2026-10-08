using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Refunds.ProcessRefund;
using MarketPlace.Api.Features.Refunds.RequestRefund;
using Microsoft.AspNetCore.Authorization;

namespace MarketPlace.Api.Features.Refunds;

public sealed class RefundsEndpoint : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("refunds").RequireAuthorization();

        group
            .MapPost(
                "/request",
                async (
                    RequestRefundRequest request,
                    RequestRefundHandler handler,
                    System.Security.Claims.ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                {
                    var response = await handler.HandleAsync(request, user, cancellationToken);
                    return response.ToMinimalApiResult();
                }
            )
            .WithName("RequestRefund")
            .WithSummary("Request a refund for an order or order item")
            .Produces<ApiResponse<RequestRefundResponse>>();

        group
            .MapPost(
                "/{id:guid}/process",
                [Authorize(Roles = "SuperAdmin")]
                async (
                    Guid id,
                    ProcessRefundRequest request,
                    ProcessRefundHandler handler,
                    System.Security.Claims.ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                {
                    var response = await handler.HandleAsync(id, request, user, cancellationToken);
                    return response.ToMinimalApiResult();
                }
            )
            .WithName("ProcessRefund")
            .WithSummary("Process a refund request (Admin only)")
            .Produces<ApiResponse<ProcessRefundResponse>>();
    }
}
