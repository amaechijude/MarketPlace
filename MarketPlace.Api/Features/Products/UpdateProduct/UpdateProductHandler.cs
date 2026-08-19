using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Infrastucture.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Features.Products.UpdateProduct;

public sealed class UpdateProductHandler(
    AppDbContext context,
    HybridCache hybridCache,
    TimeProvider timeProvider
) : IRequestHandler
{
    public async Task<ApiResponse<int>> HandleAsync(
        Guid productId,
        UpdateProductRequest request,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var product = await context.Products.FindAsync([productId], cancellationToken);

        if (product is null)
            return ApiResponse<int>.NotFound("Product not found");

        // Apply only the fields that were supplied in the request
        if (!string.IsNullOrWhiteSpace(request.Name))
            product.Name = request.Name.Trim();

        if (!string.IsNullOrWhiteSpace(request.LongDescription))
            product.LongDescription = request.LongDescription.Trim();

        if (!string.IsNullOrWhiteSpace(request.ShortDescription))
            product.ShortDescription = request.ShortDescription.Trim();

        if (request.PriceInKobo.HasValue)
            product.PriceInKobo = (long)request.PriceInKobo;

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            var slug = Slugger.Slugify(request.CategorySlug);

            var category = await context
                .Categories.AsNoTracking()
                .Where(c => c.Slug == slug)
                .Select(s => new { s.Id })
                .FirstOrDefaultAsync(cancellationToken);

            if (category is not null)
                product.CategoryId = category.Id;
        }

        product.LastUpdatedAt = timeProvider.GetUtcNow();
        product.LastUpdatedBy = userId;

        await context.SaveChangesAsync(cancellationToken);

        // Invalidate the cached product so the next read picks up fresh data
        await hybridCache.RemoveAsync(
            key: CacheKeys.Product(productId),
            cancellationToken: cancellationToken
        );

        return ApiResponse<int>.NoContent();
    }
}
