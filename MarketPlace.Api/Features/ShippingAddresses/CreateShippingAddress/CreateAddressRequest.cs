using FluentValidation;
using JetBrains.Annotations;
using MarketPlace.Api.Common.Extensions;

namespace MarketPlace.Api.Features.ShippingAddresses.CreateShippingAddress;

public sealed record CreateAddressRequest(
    string FirstName,
    string LastName,
    string Phone,
    string Address,
    string State,
    string Lga,
    string City,
    string Landmark
);

[UsedImplicitly]
public sealed class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(100)
            .MinimumLength(2);

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(100)
            .MinimumLength(2);

        RuleFor(x => x.Phone)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(16)
            .MinimumLength(11)
            .Must(IsAllAsciiDigit)
            .WithMessage("Phone number must be all digits");

        RuleFor(x => x.Landmark)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(250)
            .MinimumLength(5);

        RuleFor(x => x.Address)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(250)
            .MinimumLength(5);

        RuleFor(x => x.City)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(100)
            .MinimumLength(2);

        RuleFor(x => x.State)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(50)
            .MinimumLength(2);
    }

    private static bool IsAllAsciiDigit(string value) => value[1..].IsAllAsciiDigits();
}
