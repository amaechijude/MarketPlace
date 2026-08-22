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
                "/initiate/{addressId:guid}",
                async (
                    [FromRoute] Guid addressId,
                    [FromServices] InitiateCheckoutHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) => (await handler.HandleAsync(addressId, user.UserId, ct)).ToMinimalApiResult()
            )
            .ProducesResponsesWithProblem<CheckoutResponse>([400]);

        checkoutGroup
            .MapPost(
                "/validate/{reference}",
                async (
                    [FromRoute] string reference,
                    [FromServices] ValidateCheckoutHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) => (await handler.HandleAsync(user.UserId, reference, ct)).ToMinimalApiResult()
            )
            .ProducesResponsesWithProblem<ValidateCheckoutResponse>([503, 400]);
    }
}
