using System.Security.Claims;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Carts.RemoveFromCart;

public static class RemoveItemFromCartEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapDelete(
                "/{productId:guid}",
                async (
                    [FromRoute] Guid productId,
                    [FromServices] AppDbContext context,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                {
                    var userId = user.UserId;
                    if (userId.IsEmpty)
                        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized);

                    await context
                        .CartItems.Where(c => c.Id == productId && c.Cart.UserId == userId)
                        .ExecuteDeleteAsync(cancellationToken);

                    return Results.NoContent();
                }
            )
            .RequireAuthorization()
            .Produces(204)
            .WithDescription("Remove item");

        // clear
        group
            .MapDelete(
                "/",
                async (
                    [FromServices] AppDbContext context,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                {
                    var userId = user.UserId;
                    if (userId.IsEmpty)
                        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized);

                    await context
                        .CartItems.Where(ci => ci.Cart.UserId == userId)
                        .ExecuteDeleteAsync(cancellationToken);

                    return Results.NoContent();
                }
            )
            .RequireAuthorization()
            .Produces(204)
            .WithDescription("Clear cart");
    }
}
