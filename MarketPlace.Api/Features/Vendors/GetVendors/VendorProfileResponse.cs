using MarketPlace.Api.Domain.Entities.Enums;

namespace MarketPlace.Api.Features.Vendors.DTOs;

public sealed record VendorProfileResponse(
    Guid VendorId,
    string BusinessName,
    string StoreSlug,
    string? Description,
    string? LogoUrl,
    string SupportEmail,
    string? SupportPhone,
    VendorApprovalStatus ApprovalStatus,
    bool IsActive,
    decimal? AverageRating,
    int TotalReviews,
    DateTimeOffset CreatedAt
);
