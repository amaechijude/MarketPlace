using System.Text.Json.Serialization;
using FluentValidation;

namespace MarketPlace.Api.Features.Products.UpdateProduct;

public sealed record UpdateProductRequest(
    string? Name,
    string? LongDescription,
    string? ShortDescription,
    int? PriceInNaira,
    string? CategorySlug,
    bool? IsPublished
)
{
    [JsonIgnore]
    public long? PriceInKobo => PriceInNaira * 100;
};

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(p => p.Name)
            .MinimumLength(3)
            .WithMessage("Product name must be at least 3 characters long")
            .When(p => !string.IsNullOrWhiteSpace(p.Name));

        RuleFor(p => p.LongDescription)
            .MinimumLength(10)
            .WithMessage("Product description must be at least 10 characters long")
            .MaximumLength(2500)
            .WithMessage("Product description must be at most 2500 characters long")
            .When(p => !string.IsNullOrWhiteSpace(p.LongDescription));

        RuleFor(p => p.ShortDescription)
            .MinimumLength(10)
            .WithMessage("Product short description must be at least 10 characters long")
            .MaximumLength(300)
            .WithMessage("Product short description must be at most 300 characters long")
            .When(p => !string.IsNullOrWhiteSpace(p.ShortDescription));

        RuleFor(p => p.PriceInNaira)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Product price must be greater than or equal to 1")
            .When(p => p.PriceInNaira.HasValue);
    }
}
