using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.VendorWallets.GetWalletBalance;

public sealed record WalletBalanceResponse(
    long AvailableBalanceInKobo,
    long PendingBalanceInKobo,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public sealed class GetWalletBalanceHandler(AppDbContext dbContext) : IScopedRequestHandler
{
    public async Task<ApiResponse<WalletBalanceResponse>> HandleAsync(
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
            return ApiResponse<WalletBalanceResponse>.Forbidden("User is not a vendor");
        }

        var wallet = await dbContext.Wallets.FirstOrDefaultAsync(
            w => w.VendorId == vendorId.Value,
            cancellationToken
        );

        if (wallet is null)
        {
            // If the vendor has never received an order, wallet might not exist.
            return ApiResponse<WalletBalanceResponse>.Success(
                new WalletBalanceResponse(0, 0, DateTimeOffset.UtcNow, null)
            );
        }

        return ApiResponse<WalletBalanceResponse>.Success(
            new WalletBalanceResponse(
                wallet.AvailableBalanceInKobo,
                wallet.PendingBalanceInKobo,
                wallet.CreatedAt,
                wallet.UpdatedAt
            )
        );
    }
}
