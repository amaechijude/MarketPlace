using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.Vendors.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Vendors.Handlers;

public sealed class SuspendVendorHandler(
    AppDbContext context,
    TimeProvider timeProvider,
    ILogger<SuspendVendorHandler> logger
) : IScopedRequestHandler
{
    public async Task<ApiResponse<int>> HandleAsync(
        Guid vendorId,
        SuspendVendorRequest request,
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

        if (vendor.ApprovalStatus == Domain.Entities.Enums.VendorApprovalStatus.Suspended)
            return ApiResponse<int>.Conflict("Vendor is already suspended.");

        try
        {
            vendor.Suspend(request.Reason, adminUserId, timeProvider.GetUtcNow());
            await context.SaveChangesAsync(cancellationToken);
            return ApiResponse<int>.Success(200);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(
                ex,
                "Database update error during vendor suspension for vendor {VendorId}",
                vendorId
            );
            return ApiResponse<int>.BadRequest(
                "An error occurred while suspending the vendor. Please try again later."
            );
        }
    }
}
