using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.Products.ListProduct;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Products.SearchProduct;

public sealed class SearchProductHandler(AppDbContext context, IEmbeddingService embeddingService)
    : IScopedRequestHandler
{
    public async Task<ApiResponse<IEnumerable<ListProductResponse>>> HandleAsync(
        SearchProductRequest request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Trim().Length < 2)
            return ApiResponse<IEnumerable<ListProductResponse>>.BadRequest("Query is required.");

        var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(
            request.Query,
            cancellationToken
        );
        var pgVector = new Pgvector.Vector(queryEmbedding);

        var products = await context
            .Products.AsNoTracking()
            .Where(p => p.Embedding != null)
            .OrderBy(p => p.Embedding!.L2Distance(pgVector))
            .Take(request.Limit)
            .Select(s => new ListProductResponse(
                s.Id,
                s.Name,
                s.BasePriceInKobo,
                s.ThumbnailUrl,
                s.Category.Slug
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<IEnumerable<ListProductResponse>>.Success(products);
    }
}
