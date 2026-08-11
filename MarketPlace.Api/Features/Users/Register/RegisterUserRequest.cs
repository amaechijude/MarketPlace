using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Users.Register;

public sealed record RegisterUserRequest(
    [Required, NoInBetweenWhiteSpace, EmailAddress] string Email,
    [Required, IsValidPassword] string Password
);

public sealed record RegisterUserResponse(
    Guid OtpId,
    string Message = "Registration Successful. Check email for otp"
);

public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress()
            .Must(e => e.HasNoWhiteSpaceBetweenChar())
            .WithMessage("Email cannot contain White space in between characters");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .Must(m => m.Length >= 6)
            .WithMessage("Password must be at 6 characters")
            .Must(m => m.Any(char.IsUpper))
            .WithMessage("Password must contain Uppercase")
            .Must(m => m.Any(char.IsLower))
            .WithMessage("Password must contain Lowercase")
            .Must(m => m.Any(char.IsAsciiDigit))
            .WithMessage("Password must contain digit")
            .Must(m => m.ContainsSpecialCharacter())
            .WithMessage("Password must contain Special character");
    }
}
