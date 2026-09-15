using MarketPlace.Api.Domain.Entities;

namespace MarketPlace.Api.Features.Vendors.DTOs;

/// <summary>
/// Response DTO for vendor onboarding.
/// Provides confirmation and details of the newly registered vendor.
/// </summary>
public sealed record VendorOnboardingResponse
(
    /// <summary>Unique identifier for the newly created vendor.</summary>
    Guid VendorId,

    /// <summary>The user ID associated with this vendor.</summary>
    Guid UserId,

    /// <summary>Business name of the vendor.</summary>
    string BusinessName,

    /// <summary>Store slug assigned to the vendor.</summary>
    string StoreSlug,

    /// <summary>Current approval status (should be Pending for new vendors).</summary>
    VendorApprovalStatus ApprovalStatus,

    /// <summary>Timestamp when the vendor was registered.</summary>
    DateTimeOffset RegisteredAt,

    /// <summary>Message indicating the next steps for the vendor.</summary>
    string Message
);
