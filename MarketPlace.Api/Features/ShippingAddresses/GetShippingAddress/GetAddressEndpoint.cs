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
                ) =>
                {
                    var userId = user.UserId;
                    return userId.IsEmpty
                        ? Results.Problem(statusCode: StatusCodes.Status401Unauthorized)
                        : (await handler.HandleAsync(userId, addressId, ct)).ToMinimalApiResult();
                }
            )
            .RequireAuthorization()
            .Produces<AddressResponse>()
            .ProducesProblem(404);
    }
}
