using FluentValidation;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Infrastucture.MediaStorage.CloudflareR2;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Products.CreateProduct;

public sealed class CreateProductHandler(
    IValidator<CreateProductRequest> validator,
    AppDbContext context,
    R2ImageUploadService r2ImageUpload,
    TimeProvider timeProvider,
    ILogger<CreateProductHandler> logger
) : IScopedRequestHandler
{
    public async Task<ApiResponse<int>> HandleAsync(
        CreateProductRequest request,
        IEnumerable<string> roleList,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ApiResponse<int>.ValidationProblem(validationResult.ToDictionary());

        var slug = Slugger.Slugify(request.CategorySlug);

        var category = await context
            .Categories.AsNoTracking()
            .Where(c => c.Slug == slug)
            .Select(s => new { s.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
            return ApiResponse<int>.NotFound("Category not Found");

        var upload = await UploadImages(request.Thumbnail, request.ImageArray, cancellationToken);
        if (upload is null || upload.Count <= 0)
            return ApiResponse<int>.BadRequest("Image upload failed try again later");

        var thumbnail = upload.FirstOrDefault(u => u.Isthubnail)!;

        upload.Remove(thumbnail);

        var utcNow = timeProvider.GetUtcNow();
        var product = CreateProduct(request, userId, thumbnail, utcNow, upload, category.Id);

        product.AddVariant(request.Variants, utcNow, userId);

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return ApiResponse<int>.Created();
    }

    private static Product CreateProduct(
        CreateProductRequest request,
        Guid userId,
        ImageUploadResult thumbnail,
        DateTimeOffset utcNow,
        List<ImageUploadResult> upload,
        int categoryId
    ) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name,
            ShortDescription = request.ShortDescription,
            LongDescription = request.LongDescription,
            ThumbnailUrl = thumbnail.FileUrl,
            ThumbnailFileKey = thumbnail.FileKey,
            CreatedAt = utcNow,
            LastUpdatedAt = utcNow,
            ImageUrlsArray = [.. upload.Select(s => s.FileUrl)],
            ImageFileKeysArray = [.. upload.Select(s => s.FileKey)],
            IsPublished = request.IsPublished,
            CategoryId = categoryId,
            VendorId = userId,
            BasePriceInKobo = request.Variants.Select(s => s.PriceInKobo).First(),
        };

    private async Task<List<ImageUploadResult>?> UploadImages(
        IFormFile thumbnail,
        IFormFileCollection imageArrays,
        CancellationToken cancellationToken
    )
    {
        try
        {
            IEnumerable<UploadImageRequest> uploadRequest =
            [
                new(thumbnail, FolderName, true),
                .. imageArrays.Select(f => new UploadImageRequest(f, FolderName, false)),
            ];

            return await r2ImageUpload.UploadImageAsync(uploadRequest, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Image upload failed with reason {message}", e.Message);
            return null;
        }
    }

    private const string FolderName = "products";
}
