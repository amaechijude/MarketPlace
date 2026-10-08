using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Refunds.RequestRefund;

public sealed record RequestRefundRequest(Guid OrderId, Guid? OrderItemId, string Reason);

public sealed record RequestRefundResponse(Guid RefundRequestId, string Message);

public sealed class RequestRefundHandler(AppDbContext dbContext, TimeProvider timeProvider)
    : IScopedRequestHandler
{
    public async Task<ApiResponse<RequestRefundResponse>> HandleAsync(
        RequestRefundRequest request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        var userId = user.UserId;

        var orderQuery = dbContext.Orders.Where(o => o.Id == request.OrderId && o.UserId == userId);

        if (request.OrderItemId.HasValue)
        {
            orderQuery = orderQuery.Include(o =>
                o.OrderItems.Where(oi => oi.Id == request.OrderItemId.Value)
            );
        }
        else
        {
            orderQuery = orderQuery.Include(o => o.OrderItems);
        }

        var order = await orderQuery.FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return ApiResponse<RequestRefundResponse>.NotFound(
                "Order not found or does not belong to the user."
            );
        }

        long refundAmount = 0;

        if (request.OrderItemId.HasValue)
        {
            var item = order.OrderItems.FirstOrDefault();
            if (item is null)
                return ApiResponse<RequestRefundResponse>.NotFound("Order item not found.");

            refundAmount = item.UnitPriceInKobo * item.Quantity;
            // Optionally, handle discount distribution logic here.
            // For simplicity, we refund the item price.
        }
        else
        {
            refundAmount = order.TotalInKobo - order.ShippingFeeInKobo; // Refund total minus shipping
        }

        // Prevent duplicate pending requests
        var existingRequest = await dbContext.RefundRequests.AnyAsync(
            r =>
                r.OrderId == request.OrderId
                && r.OrderItemId == request.OrderItemId
                && r.Status == Domain.Entities.Enums.RefundStatus.Pending,
            cancellationToken
        );

        if (existingRequest)
        {
            return ApiResponse<RequestRefundResponse>.BadRequest(
                "A pending refund request already exists for this order/item."
            );
        }

        var refundRequest = new RefundRequest
        {
            OrderId = order.Id,
            OrderItemId = request.OrderItemId,
            UserId = userId,
            Reason = request.Reason,
            AmountInKobo = refundAmount,
            CreatedAt = timeProvider.GetUtcNow(),
        };

        dbContext.RefundRequests.Add(refundRequest);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<RequestRefundResponse>.Success(
            new RequestRefundResponse(refundRequest.Id, "Refund request submitted successfully.")
        );
    }
}
