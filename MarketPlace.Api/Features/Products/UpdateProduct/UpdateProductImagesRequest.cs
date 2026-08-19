using FluentValidation;
using SkiaSharp;

namespace MarketPlace.Api.Features.Products.UpdateProduct;

public sealed record UpdateProductImagesRequest(
    IFormFile Thumbnail,
    IFormFileCollection ImageArray
);

public sealed class UpdateProductImagesRequestValidator
    : AbstractValidator<UpdateProductImagesRequest>
{
    public UpdateProductImagesRequestValidator()
    {
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
