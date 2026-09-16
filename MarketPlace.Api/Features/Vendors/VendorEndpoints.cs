using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Vendors.DTOs;
using MarketPlace.Api.Features.Vendors.Handlers;
using MarketPlace.Api.Features.Vendors.Validators;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Vendors;

/// <summary>
/// Endpoint mappings for vendor management operations.
/// Provides routes for vendor registration, profile retrieval, and management.
/// </summary>
public sealed class VendorEndpoints : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/vendors").WithTags("Vendors");

        // Public endpoints
        group
            .MapGet(
                "/{vendorId:guid}",
                async (
                    [FromRoute] Guid vendorId,
                    [FromServices] GetVendorProfileHandler handler,
                    CancellationToken cancellationToken
                ) => (await handler.HandleAsync(vendorId, cancellationToken)).ToMinimalApiResult()
            )
            .ProducesResponseWithProblem<VendorProfileResponse>(404);

        group
            .MapGet(
                "/store/{storeSlug}",
                async (
                    [FromRoute] string storeSlug,
                    [FromServices] GetVendorProfileHandler handler,
                    CancellationToken cancellationToken
                ) => (await handler.HandleAsync(storeSlug, cancellationToken)).ToMinimalApiResult()
            )
            .ProducesResponseWithProblem<VendorProfileResponse>(404);

        // Protected endpoints
        var protectedGroup = group.RequireAuthorization();

        protectedGroup
            .MapPost(
                "/onboard",
                async (
                    [FromBody] VendorOnboardingRequest request,
                    [FromServices] VendorOnboardingHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                    (
                        await handler.HandleAsync(user.UserId, request, cancellationToken)
                    ).ToMinimalApiResult()
            )
            .WithValidation<VendorOnboardingRequest>()
            .ProducesResponseWithProblem<VendorOnboardingResponse>(409);
    }
}
