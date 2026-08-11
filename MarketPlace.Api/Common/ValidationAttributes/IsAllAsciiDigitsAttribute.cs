using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.Extensions;

namespace MarketPlace.Api.Common.ValidationAttributes;

[AttributeUsage(AttributeTargets.All)]
public class IsAllAsciiDigitsAttribute : ValidationAttribute
{
    public IsAllAsciiDigitsAttribute()
    {
        ErrorMessage = "The {0} field must be all digits.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string str || !str.IsAllAsciiDigits())
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

        return ValidationResult.Success;
    }
}
