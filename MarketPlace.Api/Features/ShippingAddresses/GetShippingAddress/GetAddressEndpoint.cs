using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.ShippingAddresses.CreateShippingAddress;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.ShippingAddresses.GetShippingAddress;

public static class GetAddressEndpoint
{
    public static void Map(RouteGroupBuilder builder)
    {
        builder
            .MapGet(
                "/{addressId:guid}",
                async (
                    [FromRoute] Guid addressId,
                    [FromServices] GetAddressHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) => (await handler.HandleAsync(user.UserId, addressId, ct)).ToMinimalApiResult()
            )
            .RequireAuthorization()
            .ProducesResponseWithProblem<AddressResponse>(401, 404);
    }
}
