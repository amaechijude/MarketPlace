using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
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
    public async Task<ApiResponse<VendorOnboardingResponse>> HandleAsync(
        Guid userId,
        VendorOnboardingRequest request,
        CancellationToken cancellationToken
    )
    {
        var user = await context.Users.FindAsync(userId, cancellationToken);
        if (user is null)
            return ApiResponse<VendorOnboardingResponse>.NotFound(
                "User not found. Please ensure you are logged in."
            );
        // Check if user already has a vendor account
        var existingVendor = await context
            .Vendors.AsNoTracking()
            .FirstOrDefaultAsync(v => v.UserId == userId, cancellationToken);

        if (existingVendor is not null)
            return ApiResponse<VendorOnboardingResponse>.Conflict(
                "User already has an active vendor account."
            );

        // Check if store slug is already taken
        var slugTaken = await context
            .Vendors.AsNoTracking()
            .AnyAsync(v => v.StoreSlug == request.StoreSlug.ToLowerInvariant(), cancellationToken);

        if (slugTaken)
            return ApiResponse<VendorOnboardingResponse>.Conflict(
                $"Store slug '{request.StoreSlug}' is already in use. Please choose a different one."
            );

        try
        {
            var now = timeProvider.GetUtcNow();

            // Create vendor entity
            var vendor = Vendor.Create(
                userId,
                request.BusinessName,
                request.StoreSlug,
                request.SupportEmail,
                now
            );

            // Add optional fields
            if (!string.IsNullOrWhiteSpace(request.Description))
                vendor.UpdateProfile(
                    request.Description,
                    request.SupportPhone,
                    null,
                    request.BusinessAddress,
                    request.Country,
                    null,
                    now
                );
            else if (
                !string.IsNullOrWhiteSpace(request.SupportPhone)
                || !string.IsNullOrWhiteSpace(request.BusinessAddress)
                || !string.IsNullOrWhiteSpace(request.Country)
            )
            {
                vendor.UpdateProfile(
                    null,
                    request.SupportPhone,
                    null,
                    request.BusinessAddress,
                    request.Country,
                    null,
                    now
                );
            }

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

            return ApiResponse<VendorOnboardingResponse>.Success(
                new VendorOnboardingResponse(
                    vendor.Id,
                    vendor.UserId,
                    vendor.BusinessName,
                    vendor.StoreSlug,
                    vendor.ApprovalStatus,
                    vendor.CreatedAt,
                    "Vendor registration successful. Your account is pending approval. You will receive an email notification once approved."
                )
            );
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(
                ex,
                "Database update error during vendor onboarding for user {UserId}",
                userId
            );
            return ApiResponse<VendorOnboardingResponse>.BadRequest(
                "An error occurred while registering your vendor account. Please try again later."
            );
        }
    }
}
