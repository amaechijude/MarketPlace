using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Vendors.Handlers;

public sealed class ApproveVendorHandler(
    AppDbContext context,
    TimeProvider timeProvider,
    ILogger<ApproveVendorHandler> logger
) : IScopedRequestHandler
{
    public async Task<ApiResponse<int>> HandleAsync(
        Guid vendorId,
        Guid adminUserId,
        CancellationToken cancellationToken
    )
    {
        var vendor = await context.Vendors.FirstOrDefaultAsync(
            v => v.Id == vendorId,
            cancellationToken
        );

        if (vendor is null)
            return ApiResponse<int>.NotFound("Vendor not found.");

        if (vendor.ApprovalStatus == Domain.Entities.Enums.VendorApprovalStatus.Approved)
            return ApiResponse<int>.Conflict("Vendor is already approved.");

        try
        {
            vendor.Approve(adminUserId, timeProvider.GetUtcNow());
            await context.SaveChangesAsync(cancellationToken);
            return ApiResponse<int>.Success(200);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(
                ex,
                "Database update error during vendor approval for vendor {VendorId}",
                vendorId
            );
            return ApiResponse<int>.BadRequest(
                "An error occurred while approving the vendor. Please try again later."
            );
        }
    }
}
