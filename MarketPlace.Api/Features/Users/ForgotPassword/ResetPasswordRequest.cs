using System.ComponentModel.DataAnnotations;
using FluentValidation;
using JetBrains.Annotations;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Users.ForgotPassword;

public sealed record ResetPasswordRequest
{
    [IsValidGuid]
    public Guid OtpId { get; init; }

    [Required, MinLength(6), IsAllAsciiDigits]
    public string OtpInput { get; init; } = string.Empty;

    [Required, IsValidPassword]
    public string Password { get; init; } = string.Empty;

    [Compare(nameof(Password))]
    public string ConfirmPassword { get; init; } = string.Empty;
}

[UsedImplicitly]
public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.OtpId).Must(m => m.IsNotEmpty);

        RuleFor(x => x.OtpInput)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Length(5, 6)
            .Must(m => m.IsAllAsciiDigits());

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

        RuleFor(x => x.ConfirmPassword).Equal(c => c.Password).WithMessage("Password mismatch");
    }
}
