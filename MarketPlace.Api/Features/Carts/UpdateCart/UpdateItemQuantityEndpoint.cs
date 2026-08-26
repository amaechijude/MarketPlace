using System.Security.Claims;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Carts.UpdateCart;

public static class UpdateItemQuantityEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapPut(
                "/",
                async (
                    [FromBody] UpdateCartItemRequest request,
                    [FromServices] AppDbContext context,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                {
                    var userId = user.UserId;
                    if (userId.IsEmpty)
                        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized);

                    await context
                        .CartItems.Where(ci =>
                            ci.ProductVariantId == request.ProductVariantId
                            && ci.Cart.UserId == userId
                        )
                        .ExecuteUpdateAsync(
                            s => s.SetProperty(p => p.Quantity, request.CurrentQuantity),
                            cancellationToken
                        );

                    return Results.NoContent();
                }
            )
            .RequireAuthorization()
            .WithValidation<UpdateCartItemRequest>()
            .Produces(204);
    }
}
