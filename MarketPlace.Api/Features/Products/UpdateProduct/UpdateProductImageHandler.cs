using FluentValidation;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Infrastucture.Cache;
using MarketPlace.Api.Infrastucture.MediaStorage.CloudflareR2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Features.Products.UpdateProduct;

public sealed class UpdateProductImageHandler(
    AppDbContext context,
    R2ImageUploadService r2ImageUpload,
    HybridCache hybridCache,
    TimeProvider timeProvider,
    IValidator<UpdateProductImagesRequest> validator,
    ILogger<UpdateProductImageHandler> logger
) : IRequestHandler
{
    public async Task<ApiResponse<int>> HandleAsync(
        Guid productId,
        UpdateProductImagesRequest request,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return ApiResponse<int>.ValidationProblem(validationResult.ToDictionary());

        var product = await context
            .Products.Where(p => p.Id == productId && p.CreatedBy == userId)
            .Select(s => new { s.ThumbnailFileKey, s.ImageFileKeysArray })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            ApiResponse<int>.NotFound("Product not found");

        // upload new ones

        var upload = await UploadImages(request.Thumbnail, request.ImageArray, cancellationToken);
        if (upload is null || upload.Count <= 0)
            return ApiResponse<int>.BadRequest("Image upload failed try again later");

        var thumbnail = upload.FirstOrDefault(u => u.Isthubnail)!;
        upload.Remove(thumbnail);

        // delete old ones
        await DeleteImagesasync(
            [product!.ThumbnailFileKey, .. product.ImageFileKeysArray],
            cancellationToken
        );

        var imageurls = upload.Select(f => f.FileUrl).ToArray();
        var imageFilekeys = upload.Select(f => f.FileKey).ToArray();
        var now = timeProvider.GetUtcNow();

        // update values
        await context
            .Products.Where(p => p.Id == productId)
            .ExecuteUpdateAsync(
                u =>
                    u.SetProperty(s => s.ThumbnailFileKey, thumbnail.FileKey)
                        .SetProperty(s => s.ThumbnailUrl, thumbnail.FileUrl)
                        .SetProperty(s => s.ImageUrlsArray, imageurls)
                        .SetProperty(s => s.ImageFileKeysArray, imageFilekeys)
                        .SetProperty(s => s.LastUpdatedAt, now)
                        .SetProperty(s => s.LastUpdatedBy, userId),
                cancellationToken: cancellationToken
            );

        // invalidate cache
        await hybridCache.RemoveAsync(key: CacheKeys.Product(productId), cancellationToken);

        return ApiResponse<int>.NoContent();
    }

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

    private async Task DeleteImagesasync(IEnumerable<string> filekeys, CancellationToken ct)
    {
        try
        {
            await r2ImageUpload.DeleteImageAsync(filekeys, ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Image deletion failed with reason {message}", e.Message);
            throw;
        }
    }

    private const string FolderName = "products";
}
