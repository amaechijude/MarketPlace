using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Carts.ListCartItems;

public static class ListCartItemsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/",
                async (
                    [AsParameters] ListCartItemRequest request,
                    [FromServices] ListCartItemHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) =>
                {
                    var userId = user.UserId;
                    return userId.IsEmpty
                        ? Results.Problem(statusCode: StatusCodes.Status401Unauthorized)
                        : (await handler.HandleAsync(userId, request, ct)).ToMinimalApiResult();
                }
            )
            .RequireAuthorization()
            .Produces<CursorPagedResponse<CartItemResponse>>();
    }
}
