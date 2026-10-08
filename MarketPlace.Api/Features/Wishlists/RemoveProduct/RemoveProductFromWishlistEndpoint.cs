using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Wishlists.RemoveProduct;

public static class RemoveProductFromWishlistEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapDelete(
                "/products/{productId:guid}",
                async (
                    [FromRoute] Guid productId,
                    [FromServices] RemoveProductFromWishlistHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) =>
                {
                    var request = new RemoveProductFromWishlistRequest(productId);
                    return (await handler.HandleAsync(request, user, ct)).ToMinimalApiResult();
                }
            )
            .Produces<RemoveProductFromWishlistResponse>();
    }
}
