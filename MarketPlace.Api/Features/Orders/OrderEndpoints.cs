using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Orders.DTOs;
using MarketPlace.Api.Features.Orders.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Orders;

public sealed class OrderEndpoints : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/orders").WithTags("Orders").RequireAuthorization();

        group
            .MapGet(
                "/my-orders",
                async (
                    [FromQuery] Guid? cursor,
                    [FromQuery] int pageSize,
                    [FromServices] ListMyOrdersHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                    (
                        await handler.HandleAsync(
                            user.UserId,
                            cursor,
                            pageSize == 0 ? 10 : pageSize,
                            cancellationToken
                        )
                    ).ToMinimalApiResult()
            )
            .ProducesResponseWithProblem<CursorPagedResponse<OrderResponse, Guid?>>(400);

        var adminGroup = group.MapGroup("").RequireAuthorization();

        adminGroup.MapPut(
            "/{orderId:guid}/status",
            async (
                [FromRoute] Guid orderId,
                [FromBody] UpdateOrderStatusRequest request,
                [FromServices] UpdateOrderStatusHandler handler,
                CancellationToken cancellationToken
            ) =>
                (
                    await handler.HandleAsync(orderId, request, cancellationToken)
                ).ToMinimalApiResult()
        );
    }
}
