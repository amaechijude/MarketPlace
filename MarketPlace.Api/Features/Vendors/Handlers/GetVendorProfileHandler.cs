using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.Vendors.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Vendors.Handlers;

/// <summary>
/// Handler for retrieving vendor profile information.
/// Provides comprehensive vendor details based on vendor ID or store slug.
/// </summary>
public sealed class GetVendorProfileHandler(AppDbContext context) : IScopedRequestHandler
{
    /// <summary>
    /// Retrieves vendor profile by vendor ID.
    /// </summary>
    public async Task<ApiResponse<VendorProfileResponse>> HandleAsync(
        Guid vendorId,
        CancellationToken cancellationToken
    )
    {
        var vendor = await context
            .Vendors.AsNoTracking()
            .Where(v => v.Id == vendorId)
            .Select(
                v =>
                    new VendorProfileResponse(
                        v.Id,
                        v.BusinessName,
                        v.StoreSlug,
                        v.Description,
                        v.LogoUrl,
                        v.SupportEmail,
                        v.SupportPhone,
                        v.ApprovalStatus,
                        v.IsActive,
                        v.AverageRating,
                        v.TotalReviews,
                        v.CreatedAt,
                        v.UpdatedAt
                    )
            )
            .FirstOrDefaultAsync(cancellationToken);

        return vendor is null
            ? ApiResponse<VendorProfileResponse>.NotFound(
                $"Vendor with ID {vendorId} not found."
            )
            : ApiResponse<VendorProfileResponse>.Success(vendor);
    }

    /// <summary>
    /// Retrieves vendor profile by store slug.
    /// </summary>
    public async Task<ApiResponse<VendorProfileResponse>> HandleAsync(
        string storeSlug,
        CancellationToken cancellationToken
    )
    {
        var vendor = await context
            .Vendors.AsNoTracking()
            .Where(v => v.StoreSlug == storeSlug.ToLowerInvariant())
            .Select(
                v =>
                    new VendorProfileResponse(
                        v.Id,
                        v.BusinessName,
                        v.StoreSlug,
                        v.Description,
                        v.LogoUrl,
                        v.SupportEmail,
                        v.SupportPhone,
                        v.ApprovalStatus,
                        v.IsActive,
                        v.AverageRating,
                        v.TotalReviews,
                        v.CreatedAt,
                        v.UpdatedAt
                    )
            )
            .FirstOrDefaultAsync(cancellationToken);

        return vendor is null
            ? ApiResponse<VendorProfileResponse>.NotFound(
                $"Store '{storeSlug}' not found."
            )
            : ApiResponse<VendorProfileResponse>.Success(vendor);
    }
}
