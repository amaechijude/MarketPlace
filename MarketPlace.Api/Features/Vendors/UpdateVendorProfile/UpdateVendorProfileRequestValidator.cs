using FluentValidation;
using MarketPlace.Api.Features.Vendors.DTOs;

namespace MarketPlace.Api.Features.Vendors.UpdateVendorProfile;

public sealed class UpdateVendorProfileRequestValidator
    : AbstractValidator<UpdateVendorProfileRequest>
{
    public UpdateVendorProfileRequestValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.SupportPhone)
            .MaximumLength(20)
            .WithMessage("Support phone must not exceed 20 characters.");

        RuleFor(x => x.BusinessAddress)
            .MaximumLength(500)
            .WithMessage("Business address must not exceed 500 characters.");

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .WithMessage("Country must not exceed 100 characters.");
    }
}
