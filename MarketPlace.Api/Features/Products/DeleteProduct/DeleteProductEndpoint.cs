using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Products.DeleteProduct;

public static class DeleteProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapDelete(
                "{productId:guid}",
                async (
                    [FromRoute] Guid productId,
                    [FromServices] DeleteProductHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) =>
                {
                    var userId = user.UserId;
                    return userId.IsEmpty()
                        ? Results.Problem(statusCode: StatusCodes.Status401Unauthorized)
                        : (await handler.HandleAsync(userId, productId, ct)).ToMinimalApiResult();
                }
            )
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
