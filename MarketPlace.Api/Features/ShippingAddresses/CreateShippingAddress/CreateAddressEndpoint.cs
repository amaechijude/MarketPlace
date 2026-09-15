using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Infrastucture.RateLimiting;
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
                    CancellationToken ct
                ) => (await handler.HandleAsync(user.UserId, request, ct)).ToMinimalApiResult()
            )
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicyKeys.AddressTokenBucket)
            .WithValidation<CreateAddressRequest>()
            .Produces<AddressResponse>();
    }
}
