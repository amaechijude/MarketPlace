using System.Security.Claims;
using GitgBrand.Api.Features.Checkout;
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
                "/initiate/{shippingAddressId:guid}",
                async (
                    [FromRoute] Guid shippingAddressId,
                    [FromServices] InitiateCheckoutHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) =>
                {
                    var (userId, _) = user.ResolveUserIdAndRole();
                    if (userId == Guid.Empty)
                        return Results.Problem(statusCode: 401);

                    var response = await handler.HandleAsync(shippingAddressId, userId, ct);
                    return response.ToMinimalApiResult();
                }
            )
            .Produces<CheckoutResponse>()
            .ProducesProblemWithErrorCodes([400, 401]);

        checkoutGroup
            .MapPost(
                "/validate/{reference}",
                async (
                    [FromRoute] string reference,
                    [FromServices] ValidateCheckoutHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) =>
                {
                    var (userId, _) = user.ResolveUserIdAndRole();
                    if (userId == Guid.Empty)
                        return Results.Problem(statusCode: 401);

                    var response = await handler.HandleAsync(userId, reference, ct);
                    return response.ToMinimalApiResult();
                }
            )
            .Produces<ValidateCheckoutResponse>()
            .ProducesProblemWithErrorCodes([400, 401, 402, 404, 500]);
    }
}
