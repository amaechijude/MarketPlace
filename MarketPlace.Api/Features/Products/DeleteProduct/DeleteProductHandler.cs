using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Infrastucture.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Features.Products.DeleteProduct;

public sealed class DeleteProductHandler(AppDbContext context, HybridCache hybridCache)
    : IRequestHandler
{
    public async Task<ApiResponse<int>> HandleAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken
    )
    {
        var product = await context
            .Products.Where(p => p.Id == productId && p.VendorId == userId)
            .Select(s => new { s.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            return ApiResponse<int>.NotFound("Product does not exit or is deleted");

        await context
            .Products.Where(p => p.Id == productId)
            .ExecuteUpdateAsync(u => u.SetProperty(s => s.IsPublished, false), cancellationToken);

        // Invalidate the cached product
        await hybridCache.RemoveAsync(
            key: CacheKeys.Product(productId),
            cancellationToken: cancellationToken
        );

        return ApiResponse<int>.NoContent();
    }
}
