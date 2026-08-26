using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext.SeedData;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.ShippingFees;

public sealed class ShippingFeeEndpoint : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var apiGroup = app.MapGroup("shipping-fees")
            .RequireAuthorization(p => p.RequireRole(CustomAppRoles.InventoryManager))
            .WithTags("Shipping Fees");

        apiGroup
            .MapGet(
                "/",
                async (
                    [FromServices] ListShippingFeeHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                    (
                        await handler.HandleAsync(user.UserRoles, cancellationToken)
                    ).ToMinimalApiResult()
            )
            .Produces<ListShippingFeeResponse>()
            .ProducesProblem(403);

        apiGroup
            .MapGet(
                "/{feeId:int}",
                async (
                    [FromRoute] int feeId,
                    [FromServices] GetShippingFeeHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                    (
                        await handler.HandleAsync(feeId, user.UserRoles, cancellationToken)
                    ).ToMinimalApiResult()
            )
            .Produces<ShippingFeeResponse>();

        apiGroup
            .MapPut(
                "/{feeId:int}",
                async (
                    [FromRoute] int feeId,
                    [FromBody] UpdatedShippingFeeRequest request,
                    [FromServices] UpdateShippingFeeHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                    (
                        await handler.HandleAsync(
                            feeId,
                            user.UserId,
                            user.UserRoles,
                            request,
                            cancellationToken
                        )
                    ).ToMinimalApiResult()
            )
            .WithValidation<UpdatedShippingFeeRequest>();

        apiGroup
            .MapPost(
                "/",
                async (
                    [FromBody] CreateShippingFeeRequest request,
                    [FromServices] CreateShippingFeeHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) => (await handler.HandleAsync(user.UserId, request, ct)).ToMinimalApiResult()
            )
            .WithValidation<CreateShippingFeeRequest>()
            .Produces(201);
    }
}
