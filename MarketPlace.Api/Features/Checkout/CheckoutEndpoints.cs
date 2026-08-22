using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Checkout;

public sealed class CheckoutEndpoints : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var checkoutGroup = app.MapGroup("/checkout").WithTags("Checkout").RequireAuthorization();

        checkoutGroup
            .MapPost(
                "initiate",
                async (
                    [FromBody] CheckoutRequest req,
                    [FromServices] InitiateCheckoutHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) =>
                    (await handler.HandleAsync(req.AdrressId, user.UserId, ct)).ToMinimalApiResult()
            )
            .WithValidation<ValidateCheckoutRequest>()
            .Produces<CheckoutResponse>();

        checkoutGroup
            .MapPost(
                "/validate/{reference}",
                async (
                    [FromBody] ValidateCheckoutRequest req,
                    [FromServices] ValidateCheckoutHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) =>
                    (await handler.HandleAsync(user.UserId, req.Reference, ct)).ToMinimalApiResult()
            )
            .WithValidation<ValidateCheckoutRequest>()
            .Produces<ValidateCheckoutResponse>();
    }
}
