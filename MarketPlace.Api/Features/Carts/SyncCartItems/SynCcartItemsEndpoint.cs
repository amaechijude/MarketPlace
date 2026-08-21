using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Carts.SyncCartItems;

public static class SynCcartItemsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/sync",
                async (
                    [FromBody] SyncCartItemRequest request,
                    [FromServices] SyncCartItemsHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                    (
                        await handler.SyncCartItemsAsync(user.UserId, request, cancellationToken)
                    ).ToMinimalApiResult()
            )
            .RequireAuthorization()
            .WithValidation<SyncCartItemRequest>()
            .Produces(204);
    }
}
