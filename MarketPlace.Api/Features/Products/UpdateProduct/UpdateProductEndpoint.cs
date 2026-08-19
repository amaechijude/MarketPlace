using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Products.UpdateProduct;

public static class UpdateProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapPut(
                "{productId:guid}",
                async (
                    [FromRoute] Guid productId,
                    [FromBody] UpdateProductRequest request,
                    [FromServices] UpdateProductHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) =>
                {
                    var userId = user.UserId;

                    return userId.IsEmpty()
                        ? Results.Problem(statusCode: StatusCodes.Status401Unauthorized)
                        : (
                            await handler.HandleAsync(productId, request, userId, ct)
                        ).ToMinimalApiResult();
                }
            )
            .RequireAuthorization()
            .Withvalidation<UpdateProductRequest>()
            .Produces(201);
    }
}
