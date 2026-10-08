using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Refunds.ProcessRefund;

public sealed record ProcessRefundRequest(RefundStatus Status, string? RejectionReason);

public sealed record ProcessRefundResponse(string Message);

public sealed class ProcessRefundHandler(AppDbContext dbContext, TimeProvider timeProvider)
    : IScopedRequestHandler
{
    public async Task<ApiResponse<ProcessRefundResponse>> HandleAsync(
        Guid refundRequestId,
        ProcessRefundRequest request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        // Admin authorization should be handled at the endpoint level.

        var refundRequest = await dbContext
            .RefundRequests.Include(r => r.OrderItem)
            .FirstOrDefaultAsync(r => r.Id == refundRequestId, cancellationToken);

        if (refundRequest is null)
        {
            return ApiResponse<ProcessRefundResponse>.NotFound("Refund request not found.");
        }

        if (refundRequest.Status != RefundStatus.Pending)
        {
            return ApiResponse<ProcessRefundResponse>.BadRequest(
                $"Refund request is already {refundRequest.Status}."
            );
        }

        if (
            request.Status == RefundStatus.Rejected
            && string.IsNullOrWhiteSpace(request.RejectionReason)
        )
        {
            return ApiResponse<ProcessRefundResponse>.BadRequest(
                "Rejection reason is required when rejecting a refund."
            );
        }

        var now = timeProvider.GetUtcNow();
        refundRequest.Status = request.Status;
        refundRequest.UpdatedAt = now;

        if (request.Status == RefundStatus.Rejected)
        {
            refundRequest.RejectionReason = request.RejectionReason;
        }
        else if (request.Status == RefundStatus.Approved || request.Status == RefundStatus.Refunded)
        {
            // If the item was delivered and funds were moved to available, we might need to deduct from vendor's wallet.
            // For now, we update the fulfillment status to Cancelled.
            if (refundRequest.OrderItem != null)
            {
                refundRequest.OrderItem.FulfillmentStatus = FulfillmentStatus.Cancelled;
            }
            else
            {
                // Order-level refund
                var orderItems = await dbContext
                    .OrderItems.Where(oi => oi.OrderId == refundRequest.OrderId)
                    .ToListAsync(cancellationToken);

                foreach (var item in orderItems)
                {
                    item.FulfillmentStatus = FulfillmentStatus.Cancelled;
                }
            }

            // Actual payment gateway refund logic would go here.
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<ProcessRefundResponse>.Success(
            new ProcessRefundResponse($"Refund request {request.Status.ToString().ToLower()}.")
        );
    }
}
