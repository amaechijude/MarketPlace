using System.Linq.Expressions;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.Vendors.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Vendors.Handlers;

public sealed class GetVendorProfileHandler(AppDbContext context) : IScopedRequestHandler
{
    public async Task<ApiResponse<VendorProfileResponse>> HandleAsync(
        Guid vendorId,
        CancellationToken cancellationToken
    )
    {
        var vendor = await context
            .Vendors.AsNoTracking()
            .Where(v => v.Id == vendorId)
            .Select(VendorExpression)
            .FirstOrDefaultAsync(cancellationToken);

        return vendor is null
            ? ApiResponse<VendorProfileResponse>.NotFound($"Vendor with ID {vendorId} not found.")
            : ApiResponse<VendorProfileResponse>.Success(vendor);
    }

    public async Task<ApiResponse<VendorProfileResponse>> HandleAsync(
        string storeSlug,
        CancellationToken cancellationToken
    )
    {
        var vendor = await context
            .Vendors.AsNoTracking()
            .Where(v => v.StoreSlug == storeSlug)
            .Select(VendorExpression)
            .FirstOrDefaultAsync(cancellationToken);

        return vendor is null
            ? ApiResponse<VendorProfileResponse>.NotFound($"Store '{storeSlug}' not found.")
            : ApiResponse<VendorProfileResponse>.Success(vendor);
    }

    private static readonly Expression<
        Func<Domain.Entities.Vendor, VendorProfileResponse>
    > VendorExpression = v => new VendorProfileResponse(
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
        v.CreatedAt
    );
}
