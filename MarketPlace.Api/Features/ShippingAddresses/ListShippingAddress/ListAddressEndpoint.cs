using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.ShippingAddresses.ListShippingAddress;

public static class ListAddressEndpoint
{
    public static void Map(RouteGroupBuilder builder)
    {
        builder
            .MapGet(
                "/",
                async (
                    [FromServices] ListAddressHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) => (await handler.HandleAsync(user.UserId, ct)).ToMinimalApiResult()
            )
            .RequireAuthorization()
            .Produces<ListAddressResponse>();
    }
}
