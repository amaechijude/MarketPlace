using MarketPlace.Api.Common.Extensions;
using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Common.ValidationAttributes;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public sealed class NoInBetweenWhiteSpaceAttribute : ValidationAttribute
{
    public NoInBetweenWhiteSpaceAttribute()
    {
        // Set a default error message, {0} will be replaced by the property name
        ErrorMessage = "The {0} field cannot contain whitespaces in-between characters.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string stValue || stValue.ContainsWhiteSpace())
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }

        return ValidationResult.Success;
    }
}
