using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.Vendors.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Vendors.Handlers;

public sealed class UpdateVendorProfileHandler(
    AppDbContext context,
    TimeProvider timeProvider,
    ILogger<UpdateVendorProfileHandler> logger
) : IScopedRequestHandler
{
    public async Task<ApiResponse<int>> HandleAsync(
        Guid userId,
        UpdateVendorProfileRequest request,
        CancellationToken cancellationToken
    )
    {
        var vendor = await context.Vendors.FirstOrDefaultAsync(
            v => v.UserId == userId,
            cancellationToken
        );

        if (vendor is null)
            return ApiResponse<int>.NotFound("Vendor profile not found for this user.");

        try
        {
            vendor.UpdateProfile(
                request.Description,
                request.LogoUrl,
                request.SupportPhone,
                request.BusinessAddress,
                request.Country,
                timeProvider.GetUtcNow()
            );

            await context.SaveChangesAsync(cancellationToken);
            return ApiResponse<int>.Success(200);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(
                ex,
                "Database update error during vendor profile update for user {UserId}",
                userId
            );
            return ApiResponse<int>.BadRequest(
                "An error occurred while updating your vendor profile. Please try again later."
            );
        }
    }
}
