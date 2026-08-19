using System.Linq.Expressions;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Infrastucture.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Features.Products.GetProduct;

public sealed class GetProductHandler(AppDbContext context, HybridCache hybridCache)
    : IRequestHandler
{
    public async Task<ApiResponse<GetProductResponse>> HandleAsync(
        Guid productId,
        CancellationToken cancellationToken
    )
    {
        var product = await hybridCache.GetOrCreateAsync<GetProductResponse?>(
            key: CacheKeys.Product(productId),
            factory: async ct =>
                await context
                    .Products.Where(p => p.Id == productId)
                    .Select(ProductExpression)
                    .FirstOrDefaultAsync(ct),
            options: new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(3),
                Expiration = TimeSpan.FromMinutes(10),
            },
            cancellationToken: cancellationToken
        );

        return product is null
            ? ApiResponse<GetProductResponse>.NotFound("Product not found")
            : ApiResponse<GetProductResponse>.Success(product);
    }

    private static readonly Expression<Func<Product, GetProductResponse>> ProductExpression =
        p => new GetProductResponse(
            Id: p.Id,
            Name: p.Name,
            PriceInKobo: p.PriceInKobo,
            LongDescription: p.LongDescription,
            ShortDescription: p.ShortDescription,
            ThumbnailUrl: p.ThumbnailUrl,
            ImageUrlsArray: p.ImageUrlsArray,
            CategorySlug: p.Category.Slug
        );
}
