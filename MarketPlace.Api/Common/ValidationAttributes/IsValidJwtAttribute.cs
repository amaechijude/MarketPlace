using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Common.ValidationAttributes;

[AttributeUsage(AttributeTargets.All)]
public sealed class IsValidJwtAttribute : ValidationAttribute
{
    public IsValidJwtAttribute()
    {
        // Set a default error message, {0} will be replaced by the property name
        ErrorMessage = "The {0} field must be a valid Jwt.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string stValue || stValue.Count(c => c == '.') != 2)
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

        return ValidationResult.Success;
    }
}
