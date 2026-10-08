using System.Linq.Expressions;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.Vendors.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Vendors.Handlers;

public sealed class ListVendorsHandler(AppDbContext context) : IScopedRequestHandler
{
    public async Task<ApiResponse<CursorPagedResponse<VendorProfileResponse, Guid?>>> HandleAsync(
        ListVendorsRequest request,
        CancellationToken cancellationToken
    )
    {
        var pageSize = Math.Clamp(request.PageSize, 10, 50);

        var query = context.Vendors.AsNoTracking().Where(v => v.IsActive);

        if (request.Cursor.HasValue)
        {
            query = query.Where(v => v.Id.CompareTo(request.Cursor.Value) > 0);
        }

        var vendors = await query
            .OrderBy(v => v.Id)
            .Take(pageSize + 1)
            .Select(VendorExpression)
            .ToListAsync(cancellationToken);

        var hasNextPage = vendors.Count > pageSize;
        if (hasNextPage)
        {
            vendors.RemoveAt(vendors.Count - 1);
        }

        Guid? nextCursor = hasNextPage ? vendors[^1].VendorId : null;

        var response = new CursorPagedResponse<VendorProfileResponse, Guid?>(
            Items: vendors,
            NextCursor: nextCursor,
            HasNextPage: hasNextPage
        );

        return ApiResponse<CursorPagedResponse<VendorProfileResponse, Guid?>>.Success(response);
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
