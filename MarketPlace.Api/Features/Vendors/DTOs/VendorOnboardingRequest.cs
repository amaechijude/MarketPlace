namespace MarketPlace.Api.Features.Vendors.DTOs;

/// <summary>
/// Request DTO for vendor onboarding/registration.
/// Captures all required information for a vendor to register on the marketplace.
/// </summary>
public sealed record VendorOnboardingRequest
(
    /// <summary>Official business name of the vendor.</summary>
    string BusinessName,

    /// <summary>URL-friendly slug for the vendor's store (e.g., "john-electronics").</summary>
    string StoreSlug,

    /// <summary>Detailed description of the vendor's business.</summary>
    string? Description,

    /// <summary>Primary email for customer support.</summary>
    string SupportEmail,

    /// <summary>Phone number for customer support.</summary>
    string? SupportPhone,

    /// <summary>Optional business registration or tax ID.</summary>
    string? BusinessRegistrationNumber,

    /// <summary>Physical address of the business headquarters.</summary>
    string? BusinessAddress,

    /// <summary>Country of operation.</summary>
    string? Country
);
