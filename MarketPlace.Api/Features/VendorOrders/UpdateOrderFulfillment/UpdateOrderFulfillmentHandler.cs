using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.VendorOrders.UpdateOrderFulfillment;

public sealed record UpdateOrderFulfillmentRequest(FulfillmentStatus Status);

public sealed record UpdateOrderFulfillmentResponse(string Message);

public sealed class UpdateOrderFulfillmentHandler(AppDbContext dbContext, TimeProvider timeProvider)
    : IScopedRequestHandler
{
    public async Task<ApiResponse<UpdateOrderFulfillmentResponse>> HandleAsync(
        Guid orderItemId,
        UpdateOrderFulfillmentRequest request,
        ClaimsPrincipal user,
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
            return ApiResponse<UpdateOrderFulfillmentResponse>.Forbidden("User is not a vendor");
        }

        var orderItem = await dbContext.OrderItems.FirstOrDefaultAsync(
            oi => oi.Id == orderItemId && oi.VendorId == vendorId.Value,
            cancellationToken
        );

        if (orderItem is null)
        {
            return ApiResponse<UpdateOrderFulfillmentResponse>.NotFound("Order item not found");
        }

        orderItem.FulfillmentStatus = request.Status;
        if (request.Status == FulfillmentStatus.Delivered)
        {
            var wallet = await dbContext.Wallets.FirstOrDefaultAsync(
                w => w.VendorId == vendorId.Value,
                cancellationToken
            );
            if (wallet != null)
            {
                var amount = orderItem.UnitPriceInKobo * orderItem.Quantity;
                wallet.MovePendingToAvailable(amount, timeProvider.GetUtcNow());

                dbContext.WalletTransactions.Add(
                    new Domain.Entities.WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Wallet = wallet,
                        AmountInKobo = amount,
                        Type = TransactionType.Credit,
                        Reference = orderItem.Id.ToString(),
                        Description = $"Payout for delivered item {orderItem.Id}",
                        CreatedAt = timeProvider.GetUtcNow(),
                    }
                );
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<UpdateOrderFulfillmentResponse>.Success(
            new UpdateOrderFulfillmentResponse("Fulfillment status updated")
        );
    }
}
