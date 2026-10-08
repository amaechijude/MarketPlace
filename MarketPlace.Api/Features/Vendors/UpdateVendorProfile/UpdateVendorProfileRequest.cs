namespace MarketPlace.Api.Features.Vendors.DTOs;

public sealed record UpdateVendorProfileRequest(
    string? Description,
    string? LogoUrl,
    string? SupportPhone,
    string? BusinessAddress,
    string? Country
);
