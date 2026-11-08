using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Auth.ForgotPassword;

public sealed record ResetPasswordRequest
{
    [Required, EmailAddress, NoInBetweenWhiteSpace]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(6), IsAllAsciiDigits]
    public string OtpInput { get; init; } = string.Empty;

    [Required, IsValidPassword]
    public string Password { get; init; } = string.Empty;

    [UsedImplicitly]
    [Compare(nameof(Password), ErrorMessage = "Password mismatch")]
    public string ConfirmPassword { get; init; } = string.Empty;
}
