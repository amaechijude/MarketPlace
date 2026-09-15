using MarketPlace.Api.Domain.Entities;

namespace MarketPlace.Api.Features.Vendors.DTOs;

/// <summary>
/// Response DTO for retrieving vendor profile information.
/// Provides comprehensive vendor details visible to customers and admins.
/// </summary>
public sealed record VendorProfileResponse
(
    /// <summary>Unique vendor identifier.</summary>
    Guid VendorId,

    /// <summary>Business name.</summary>
    string BusinessName,

    /// <summary>Store slug for URL generation.</summary>
    string StoreSlug,

    /// <summary>Business description.</summary>
    string? Description,

    /// <summary>Logo URL for branding.</summary>
    string? LogoUrl,

    /// <summary>Support email address.</summary>
    string SupportEmail,

    /// <summary>Support phone number.</summary>
    string? SupportPhone,

    /// <summary>Current approval status.</summary>
    VendorApprovalStatus ApprovalStatus,

    /// <summary>Active status.</summary>
    bool IsActive,

    /// <summary>Average rating from customers.</summary>
    decimal? AverageRating,

    /// <summary>Total number of customer reviews.</summary>
    int TotalReviews,

    /// <summary>Account creation timestamp.</summary>
    DateTimeOffset CreatedAt,

    /// <summary>Last update timestamp.</summary>
    DateTimeOffset UpdatedAt
);
