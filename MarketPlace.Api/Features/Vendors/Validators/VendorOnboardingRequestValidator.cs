using FluentValidation;
using MarketPlace.Api.Features.Vendors.DTOs;

namespace MarketPlace.Api.Features.Vendors.Validators;

/// <summary>
/// Validation rules for vendor onboarding requests.
/// Ensures all submitted data meets business requirements.
/// </summary>
public sealed class VendorOnboardingRequestValidator : AbstractValidator<VendorOnboardingRequest>
{
    private static readonly HashSet<char> AllowedSlugCharacters = new()
    {
        '-', '_'
    };

    public VendorOnboardingRequestValidator()
    {
        // Business Name validation
        RuleFor(x => x.BusinessName)
            .NotEmpty()
            .WithMessage("Business name is required.")
            .Length(3, 256)
            .WithMessage("Business name must be between 3 and 256 characters.")
            .Matches(@"^[a-zA-Z0-9\s&',.()-]+$")
            .WithMessage("Business name contains invalid characters.");

        // Store Slug validation
        RuleFor(x => x.StoreSlug)
            .NotEmpty()
            .WithMessage("Store slug is required.")
            .Length(3, 128)
            .WithMessage("Store slug must be between 3 and 128 characters.")
            .Matches(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$")
            .WithMessage(
                "Store slug must start and end with lowercase letter or number, and can only contain lowercase letters, numbers, and hyphens."
            )
            .Must(ValidateStoreSlugFormat)
            .WithMessage("Store slug format is invalid.");

        // Description validation
        RuleFor(x => x.Description)
            .MaximumLength(1024)
            .WithMessage("Description must not exceed 1024 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        // Support Email validation
        RuleFor(x => x.SupportEmail)
            .NotEmpty()
            .WithMessage("Support email is required.")
            .EmailAddress()
            .WithMessage("Support email must be a valid email address.")
            .MaximumLength(256)
            .WithMessage("Support email must not exceed 256 characters.");

        // Support Phone validation
        RuleFor(x => x.SupportPhone)
            .Matches(@"^[+]?[0-9]{1,3}[-\s]?[(]?[0-9]{1,4}[)]?[-\s]?[0-9]{1,4}[-\s]?[0-9]{1,9}$")
            .WithMessage("Support phone must be a valid phone number.")
            .MaximumLength(20)
            .WithMessage("Support phone must not exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SupportPhone));

        // Business Registration Number validation
        RuleFor(x => x.BusinessRegistrationNumber)
            .Matches(@"^[a-zA-Z0-9/\-]{5,100}$")
            .WithMessage("Business registration number format is invalid.")
            .MaximumLength(100)
            .WithMessage("Business registration number must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.BusinessRegistrationNumber));

        // Business Address validation
        RuleFor(x => x.BusinessAddress)
            .MaximumLength(512)
            .WithMessage("Business address must not exceed 512 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.BusinessAddress));

        // Country validation
        RuleFor(x => x.Country)
            .Matches(@"^[a-zA-Z\s]{2,100}$")
            .WithMessage("Country name must contain only letters and spaces.")
            .MaximumLength(100)
            .WithMessage("Country must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Country));
    }

    /// <summary>
    /// Validates the store slug format.
    /// Ensures it doesn't contain invalid character combinations.
    /// </summary>
    private static bool ValidateStoreSlugFormat(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        // Check for consecutive hyphens
        if (slug.Contains("--"))
            return false;

        // Check that it starts and ends with alphanumeric
        if (!char.IsLetterOrDigit(slug[0]) || !char.IsLetterOrDigit(slug[^1]))
            return false;

        return true;
    }
}
