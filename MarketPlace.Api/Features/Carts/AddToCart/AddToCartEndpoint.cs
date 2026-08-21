using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Carts.AddToCart;

public static class AddToCartEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/",
                async (
                    [FromBody] AddToCartRequest request,
                    [FromServices] AddToCartHandler handler,
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
            .WithValidation<AddToCartRequest>()
            .Produces(201);
    }
}
