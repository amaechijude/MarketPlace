using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FluentValidation;
using JetBrains.Annotations;
using SkiaSharp;

namespace MarketPlace.Api.Features.Products.CreateProduct;

public sealed record CreateProductRequest(
    [Required, MinLength(3)] string Name,
    [Required, MinLength(10)] string LongDescription,
    [Required, MinLength(10), MaxLength(300)] string ShortDescription,
    int PriceInNaira,
    string CategorySlug,
    IFormFile Thumbnail,
    IFormFileCollection ImageArray,
    ICollection<CreateVariantRequest> Variants,
    bool IsPublished = false
)
{
    [JsonIgnore]
    public long PriceInKobo => PriceInNaira * 100;
};

[UsedImplicitly]
public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(p => p.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Product name cannot be empty")
            .MinimumLength(3)
            .WithMessage("Product name must be at least 3 characters long");

        RuleFor(p => p.LongDescription)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Product description cannot be empty")
            .MinimumLength(10)
            .WithMessage("Product description must be at least 10 characters long");

        RuleFor(p => p.ShortDescription)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Product short description cannot be empty")
            .MinimumLength(10)
            .WithMessage("Product short description must be at least 10 characters long")
            .MaximumLength(300)
            .WithMessage("Product short description must be at most 300 characters long");

        RuleFor(p => p.PriceInNaira)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Product price must be greater than or equal to 1");

        RuleFor(p => p.CategorySlug)
            .NotNull()
            .NotEmpty()
            .WithMessage("Category slug cannot be empty");

        RuleFor(p => p.Thumbnail)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Thumbnail is required")
            .Must(f => f.Length > 0)
            .WithMessage("Thumbnail cannont be empty file")
            .Must(f => f.Length <= MaxImageBytes)
            .WithMessage(
                $"Thumbnail file size cannot be greater than {MaxImageBytes / 1024 / 1024} MB"
            )
            .Must(IsValidMimeType)
            .WithMessage("Invalid Mime Type")
            .Must(IsValidImage)
            .WithMessage("Invalid thumbnail image");

        RuleFor(p => p.ImageArray)
            .Cascade(CascadeMode.Stop)
            .Must(a => a is { Count: > 0 } and { Count: <= 4 })
            .WithMessage("Maximum of 4 images are allowed")
            .DependentRules(() =>
            {
                RuleForEach(p => p.ImageArray)
                    .Cascade(CascadeMode.Stop)
                    .Must(f => f.Length > 0)
                    .WithMessage("Image file cannot be empty")
                    .Must(f => f.Length <= MaxImageBytes)
                    .WithMessage($"Each image must be less than {MaxImageBytes / 1024 / 1024}MB")
                    .Must(IsValidMimeType)
                    .WithMessage("Invalid Mime Type")
                    .Must(IsValidImage)
                    .WithMessage("Invalid image");
            });

        RuleFor(p => p.Variants)
            .Cascade(CascadeMode.Stop)
            .Must(v => v.Count >= 1)
            .WithMessage("Product must have at leas one variant")
            .DependentRules(() =>
            {
                RuleForEach(p => p.Variants).SetValidator(new CreateProductVariantValidator());
            });
    }

    private const long MaxImageBytes = 7L * 1024 * 1024; // 7 MB

    private static readonly string[] AcceptedMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/avif",
    ];

    private static bool IsValidImage(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();

            using var codec = SKCodec.Create(stream);

            return codec is not null;
        }
        catch
        {
            // exception is thrown
            return false;
        }
    }

    private static bool IsValidMimeType(IFormFile file) =>
        AcceptedMimeTypes.Contains(file.ContentType);
}

public sealed record CreateVariantRequest(
    int PriceInNaira,
    int Quantity,
    Dictionary<string, string> Attributes
)
{
    [JsonIgnore]
    internal int PriceInKobo => PriceInNaira / 100;
};

public sealed class CreateProductVariantValidator : AbstractValidator<CreateVariantRequest>
{
    public CreateProductVariantValidator()
    {
        RuleFor(v => v.PriceInNaira).GreaterThanOrEqualTo(1);
        RuleFor(v => v.Quantity).GreaterThanOrEqualTo(1);

        RuleForEach(v => v.Attributes)
            .Cascade(CascadeMode.Stop)
            .Must(a => a.Key.Length >= 2)
            .WithMessage("An Attribute key must contain at least 2 characters")
            .Must(a => a.Value.Length >= 2)
            .WithMessage("An Attribute value must contain at least 2 characters")
            .When(v => v.Attributes.Count > 0);
    }
}
