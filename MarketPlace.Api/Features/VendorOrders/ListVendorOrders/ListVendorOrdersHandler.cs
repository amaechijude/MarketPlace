using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.VendorOrders.ListVendorOrders;

public sealed record VendorOrderItemResponse(
    Guid Id,
    Guid OrderId,
    string ProductName,
    string Sku,
    int Quantity,
    long UnitPriceInKobo,
    FulfillmentStatus FulfillmentStatus,
    DateTimeOffset CreatedAt
);

public sealed class ListVendorOrdersHandler(AppDbContext dbContext) : IScopedRequestHandler
{
    public async Task<ApiResponse<CursorPagedResponse<VendorOrderItemResponse, Guid?>>> HandleAsync(
        ClaimsPrincipal user,
        Guid? cursor,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        var userId = user.UserId;

        var vendorId = await dbContext
            .Vendors.Where(v => v.UserId == userId)
            .Select(v => (Guid?)v.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (vendorId is null)
        {
            return ApiResponse<CursorPagedResponse<VendorOrderItemResponse, Guid?>>.Forbidden(
                "User is not a vendor"
            );
        }

        pageSize = Math.Clamp(pageSize, 10, 50);

        var query = dbContext.OrderItems.AsNoTracking().Where(oi => oi.VendorId == vendorId.Value);

        if (cursor.HasValue)
        {
            query = query.Where(oi => oi.Id.CompareTo(cursor.Value) > 0);
        }

        var items = await query
            .OrderByDescending(oi => oi.CreatedAt)
            .Take(pageSize + 1)
            .Select(oi => new VendorOrderItemResponse(
                oi.Id,
                oi.OrderId,
                oi.ProductName,
                oi.Sku,
                oi.Quantity,
                oi.UnitPriceInKobo,
                oi.FulfillmentStatus,
                oi.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var hasNextPage = items.Count > pageSize;
        if (hasNextPage)
        {
            items.RemoveAt(items.Count - 1);
        }

        Guid? nextCursor = hasNextPage ? items[^1].Id : null;

        return ApiResponse<CursorPagedResponse<VendorOrderItemResponse, Guid?>>.Success(
            new CursorPagedResponse<VendorOrderItemResponse, Guid?>(items, nextCursor, hasNextPage)
        );
    }
}
