using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.VendorOrders.ListVendorOrders;
using MarketPlace.Api.Features.VendorOrders.UpdateOrderFulfillment;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.VendorOrders;

public sealed class VendorOrdersEndpoint : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("vendor/orders")
            .WithTags("Vendor Orders")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [FromQuery] Guid? cursor,
                    [FromQuery] int pageSize,
                    [FromServices] ListVendorOrdersHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                    (
                        await handler.HandleAsync(
                            user,
                            cursor,
                            pageSize == 0 ? 10 : pageSize,
                            cancellationToken
                        )
                    ).ToMinimalApiResult()
            )
            .ProducesResponseWithProblem<CursorPagedResponse<VendorOrderItemResponse, Guid?>>(400);

        group
            .MapPut(
                "/{orderItemId:guid}/fulfillment",
                async (
                    [FromRoute] Guid orderItemId,
                    [FromBody] UpdateOrderFulfillmentRequest request,
                    [FromServices] UpdateOrderFulfillmentHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                    (
                        await handler.HandleAsync(orderItemId, request, user, cancellationToken)
                    ).ToMinimalApiResult()
            )
            .ProducesResponseWithProblem<UpdateOrderFulfillmentResponse>(400);
    }
}
