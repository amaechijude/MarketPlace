namespace MarketPlace.Api.Features.Vendors.DTOs;

public sealed record VendorOnboardingRequest(
    string BusinessName,
    string StoreSlug,
    string? Description,
    string SupportEmail,
    string? SupportPhone,
    string? BusinessRegistrationNumber,
    string? BusinessAddress,
    string? Country
);
