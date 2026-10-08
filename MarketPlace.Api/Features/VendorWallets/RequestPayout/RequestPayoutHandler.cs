using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.VendorWallets.RequestPayout;

public sealed record RequestPayoutRequest(
    long AmountInKobo,
    string BankAccountName,
    string BankAccountNumber,
    string BankName
);

public sealed record RequestPayoutResponse(Guid PayoutRequestId, string Message);

public sealed class RequestPayoutHandler(AppDbContext dbContext, TimeProvider timeProvider)
    : IScopedRequestHandler
{
    public async Task<ApiResponse<RequestPayoutResponse>> HandleAsync(
        RequestPayoutRequest request,
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
            return ApiResponse<RequestPayoutResponse>.Forbidden("User is not a vendor");
        }

        var wallet = await dbContext.Wallets.FirstOrDefaultAsync(
            w => w.VendorId == vendorId.Value,
            cancellationToken
        );

        if (wallet is null || wallet.AvailableBalanceInKobo < request.AmountInKobo)
        {
            return ApiResponse<RequestPayoutResponse>.BadRequest("Insufficient available balance.");
        }

        var now = timeProvider.GetUtcNow();

        // Deduct from available balance immediately to prevent double spending
        wallet.DebitAvailable(request.AmountInKobo, now);

        var payoutRequest = new PayoutRequest
        {
            WalletId = wallet.Id,
            Wallet = wallet,
            AmountInKobo = request.AmountInKobo,
            BankAccountName = request.BankAccountName,
            BankAccountNumber = request.BankAccountNumber,
            BankName = request.BankName,
            CreatedAt = now,
        };

        dbContext.PayoutRequests.Add(payoutRequest);

        dbContext.WalletTransactions.Add(
            new WalletTransaction
            {
                WalletId = wallet.Id,
                Wallet = wallet,
                AmountInKobo = request.AmountInKobo,
                Type = Domain.Entities.Enums.TransactionType.Debit,
                Reference = payoutRequest.Id.ToString(),
                Description = "Payout Request",
                CreatedAt = now,
            }
        );

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<RequestPayoutResponse>.Success(
            new RequestPayoutResponse(payoutRequest.Id, "Payout requested successfully.")
        );
    }
}
