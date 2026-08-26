using System.Text.Json.Serialization;
using FluentValidation;

namespace MarketPlace.Api.Features.Products.UpdateProduct;

public sealed record UpdateProductRequest(
    string? Name,
    string? LongDescription,
    string? ShortDescription,
    string? CategorySlug,
    bool? IsPublished
);

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
    }
}
