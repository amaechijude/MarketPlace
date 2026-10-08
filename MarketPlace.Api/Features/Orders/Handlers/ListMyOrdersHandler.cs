using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.Orders.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Orders.Handlers;

public sealed class ListMyOrdersHandler(AppDbContext context) : IScopedRequestHandler
{
    public async Task<ApiResponse<CursorPagedResponse<OrderResponse, Guid?>>> HandleAsync(
        Guid userId,
        Guid? cursor,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        pageSize = Math.Clamp(pageSize, 10, 50);

        var query = context
            .Orders.AsNoTracking()
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId);

        if (cursor.HasValue)
        {
            query = query.Where(o => o.Id.CompareTo(cursor.Value) > 0);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Take(pageSize + 1)
            .Select(o => new OrderResponse(
                o.Id,
                o.Status,
                o.SubtotalInKobo,
                o.ShippingFeeInKobo,
                o.TotalInKobo,
                o.TrackingNumber,
                o.PaymentReference,
                o.CreatedAt,
                o.OrderItems.Select(i => new OrderItemResponse(
                    i.Id,
                    i.ProductName,
                    i.Sku,
                    i.Quantity,
                    i.UnitPriceInKobo
                ))
            ))
            .ToListAsync(cancellationToken);

        var hasNextPage = orders.Count > pageSize;
        if (hasNextPage)
        {
            orders.RemoveAt(orders.Count - 1);
        }

        Guid? nextCursor = hasNextPage ? orders[^1].Id : null;

        return ApiResponse<CursorPagedResponse<OrderResponse, Guid?>>.Success(
            new CursorPagedResponse<OrderResponse, Guid?>(orders, nextCursor, hasNextPage)
        );
    }
}
