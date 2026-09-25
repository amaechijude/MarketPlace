using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.DatabaseContext.SeedData;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Features.Vendors.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Vendors.Handlers;

public sealed class VendorOnboardingHandler(
    AppDbContext context,
    TimeProvider timeProvider,
    ILogger<VendorOnboardingHandler> logger
) : IScopedRequestHandler
{
    public async Task<ApiResponse<int>> HandleAsync(
        Guid userId,
        VendorOnboardingRequest request,
        CancellationToken cancellationToken
    )
    {
        var user = await context.Users.FindAsync([userId], cancellationToken: cancellationToken);
        if (user is null)
            return ApiResponse<int>.NotFound("User not found. Please ensure you are logged in.");
        // Check if user already has a vendor account
        var existingVendor = await context
            .Vendors.AsNoTracking()
            .FirstOrDefaultAsync(v => v.UserId == userId, cancellationToken);

        if (existingVendor is not null)
            return ApiResponse<int>.Conflict("User already has an active vendor account.");

        var slug = Slugger.Slugify(request.BusinessName);

        // Check if store slug is already taken
        var slugTaken = await context
            .Vendors.AsNoTracking()
            .AnyAsync(v => v.StoreSlug == slug, cancellationToken);

        if (slugTaken)
            return ApiResponse<int>.Conflict(
                $"Store slug '{slug}' is already in use. Please choose a different one."
            );

        try
        {
            var now = timeProvider.GetUtcNow();

            // Create vendor entity
            var vendor = Vendor.Create(userId, request.BusinessName, request.SupportEmail, now);

            if (!string.IsNullOrWhiteSpace(request.BusinessRegistrationNumber))
                vendor.UpdateBusinessRegistrationNumber(
                    request.BusinessRegistrationNumber,
                    userId,
                    now
                );

            // Persist vendor to database
            context.Vendors.Add(vendor);

            // Assign Vendor role to user
            // Get or create the Vendor role
            var vendorRole = await context.Roles.FirstOrDefaultAsync(
                r => r.Name == CustomAppRoles.Vendor,
                cancellationToken
            );

            if (vendorRole is null)
            {
                // Create the Vendor role if it doesn't exist (fallback for seeding issues)
                vendorRole = Role.Create(CustomAppRoles.Vendor, Guid.Empty);
                context.Roles.Add(vendorRole);
            }

            // Add role to user if not already assigned
            if (!user.Roles.Any(r => r.Name == CustomAppRoles.Vendor))
                user.Roles.Add(vendorRole);

            // Save all changes atomically
            await context.SaveChangesAsync(cancellationToken);

            return ApiResponse<int>.Created();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(
                ex,
                "Database update error during vendor onboarding for user {UserId}",
                userId
            );
            return ApiResponse<int>.BadRequest(
                "An error occurred while registering your vendor account. Please try again later."
            );
        }
    }
}
