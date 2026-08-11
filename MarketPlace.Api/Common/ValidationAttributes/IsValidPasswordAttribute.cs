using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.Extensions;

namespace MarketPlace.Api.Common.ValidationAttributes;

[AttributeUsage(AttributeTargets.All)]
public sealed class IsValidPasswordAttribute : ValidationAttribute
{
    private const int MaxLength = 70;
    private const int MinLength = 6;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string password)
            return new ValidationResult("Password is required");

        if (password.Length < MinLength)
            return new ValidationResult($"Password must be at least {MinLength} characters long");

        if (password.Length > MaxLength)
            return new ValidationResult(
                $"Password must be no more than {MaxLength} characters long"
            );

        if (!password.Any(char.IsUpper))
            return new ValidationResult("Password must contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            return new ValidationResult("Password must contain at least one lowercase letter");

        if (!password.Any(char.IsAsciiDigit))
            return new ValidationResult("Password must contain at least one digit");

        if (!password.ContainsSpecialCharacter())
            return new ValidationResult("Password must contain at least one special character");

        return ValidationResult.Success;
    }
}
