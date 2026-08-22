using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.ShippingAddresses.CreateShippingAddress;

public static class CreateAddressEndpoint
{
    public static void Map(RouteGroupBuilder builder)
    {
        builder
            .MapPost(
                "/",
                async (
                    [FromBody] CreateAddressRequest request,
                    [FromServices] CreateAddressHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                {
                    var userId = user.UserId;
                    return userId.IsEmpty
                        ? Results.Problem(statusCode: StatusCodes.Status401Unauthorized)
                        : (
                            await handler.HandleAsync(userId, request, cancellationToken)
                        ).ToMinimalApiResult();
                }
            )
            .RequireAuthorization()
            .WithValidation<CreateAddressRequest>()
            .Produces<AddressResponse>();
    }
}
