using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Products.ListProduct;

public sealed class ListProductHandler(AppDbContext context) : IScopedRequestHandler
{
    public async Task<ApiResponse<CursorPagedResponse<ListProductResponse>>> HandleAsync(
        ListProductRequest request,
        CancellationToken cancellationToken
    )
    {
        var pageSize = Math.Clamp(request.PageSize, 10, 50);

        var query = context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            var categorySlug = Slugger.Slugify(request.CategorySlug);

            query = query.Where(p => p.Category.Slug == categorySlug);
        }

        if (request.Cursor.HasValue)
            query = query.Where(p => p.Id.CompareTo(request.Cursor.Value) > 0);

        var products = await query
            .OrderBy(p => p.Id)
            .Take(pageSize + 1)
            .Select(s => new ListProductResponse(
                s.Id,
                s.Name,
                s.BasePriceInKobo,
                s.ThumbnailUrl,
                s.Category.Slug
            ))
            .ToListAsync(cancellationToken);

        var count = products.Count;
        var hasNextPage = count > pageSize;
        if (hasNextPage)
            products.RemoveAt(count - 1);

        Guid? nextCursor = hasNextPage ? products[^1].Id : null;

        var response = new CursorPagedResponse<ListProductResponse>(
            Items: products,
            NextCursor: nextCursor,
            HasNextPage: hasNextPage
        );
        return ApiResponse<CursorPagedResponse<ListProductResponse>>.Success(response);
    }
}
