using System.ComponentModel.DataAnnotations;
using FluentValidation;
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
    bool IsPublished = false
)
{
    public long PriceInKobo => PriceInNaira * 100;
};

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
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
    }
}
