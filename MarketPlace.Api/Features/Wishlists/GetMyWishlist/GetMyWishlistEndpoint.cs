using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Wishlists.GetMyWishlist;

public static class GetMyWishlistEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/",
                async (
                    [FromServices] GetMyWishlistHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) =>
                {
                    var request = new GetMyWishlistRequest();
                    return (await handler.HandleAsync(request, user, ct)).ToMinimalApiResult();
                }
            )
            .Produces<IEnumerable<GetMyWishlistResponseItem>>();
    }
}
