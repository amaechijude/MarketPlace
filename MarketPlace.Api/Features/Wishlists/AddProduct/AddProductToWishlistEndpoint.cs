using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Wishlists.AddProduct;

public static class AddProductToWishlistEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/products/{productId:guid}",
                async (
                    [FromRoute] Guid productId,
                    [FromServices] AddProductToWishlistHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) =>
                {
                    var request = new AddProductToWishlistRequest(productId);
                    return (await handler.HandleAsync(request, user, ct)).ToMinimalApiResult();
                }
            )
            .Produces<AddProductToWishlistResponse>();
    }
}
