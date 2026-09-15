using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Vendors.DTOs;
using MarketPlace.Api.Features.Vendors.Handlers;
using MarketPlace.Api.Features.Vendors.Validators;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            .MapGet("/{vendorId:guid}", GetVendorByIdHandler)
            .WithName("GetVendorById")
            .WithOpenApi()
            .Produces<VendorProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group
            .MapGet("/store/{storeSlug}", GetVendorBySlugHandler)
            .WithName("GetVendorBySlug")
            .WithOpenApi()
            .Produces<VendorProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Protected endpoints
        var protectedGroup = group.RequireAuthorization();

        protectedGroup
            .MapPost("/onboard", OnboardVendorHandler)
            .WithName("OnboardVendor")
            .WithOpenApi()
            .WithValidation<VendorOnboardingRequest>()
            .Produces<VendorOnboardingResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);
    }

    /// <summary>
    /// Gets vendor profile by vendor ID.
    /// </summary>
    private static async Task<IResult> GetVendorByIdHandler(
        [FromRoute] Guid vendorId,
        [FromServices] GetVendorProfileHandler handler,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.HandleAsync(vendorId, cancellationToken);
        return result.ToMinimalApiResult();
    }

    /// <summary>
    /// Gets vendor profile by store slug.
    /// </summary>
    private static async Task<IResult> GetVendorBySlugHandler(
        [FromRoute] string storeSlug,
        [FromServices] GetVendorProfileHandler handler,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.HandleAsync(storeSlug, cancellationToken);
        return result.ToMinimalApiResult();
    }

    /// <summary>
    /// Registers a user as a vendor.
    /// </summary>
    private static async Task<IResult> OnboardVendorHandler(
        [FromBody] VendorOnboardingRequest request,
        [FromServices] VendorOnboardingHandler handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.HandleAsync(user.UserId, request, cancellationToken);
        return result.ToMinimalApiResult();
    }
}
