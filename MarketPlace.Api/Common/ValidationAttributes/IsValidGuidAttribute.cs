using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.Extensions;

namespace MarketPlace.Api.Common.ValidationAttributes;

[AttributeUsage(AttributeTargets.All)]
public class IsValidGuidAttribute : ValidationAttribute
{
    public IsValidGuidAttribute()
    {
        ErrorMessage = "The {0} field must be a valid non-empty GUID.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not Guid guid || guid.IsEmpty())
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

        return ValidationResult.Success;
    }
}
