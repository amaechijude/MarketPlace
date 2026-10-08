using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Categories;

public sealed class ListCategoriesHandler(AppDbContext context) : IScopedRequestHandler
{
    public async Task<ApiResponse<CursorPagedResponse<CategoryResponse, int?>>> HandleAsync(
        ListCategoriesRequest request,
        CancellationToken cancellationToken
    )
    {
        var pageSize = Math.Clamp(request.PageSize, 20, 30);

        var query = context.Categories.AsNoTracking();
        if (request.Cursor.HasValue)
            query = query.Where(c => c.Id.CompareTo(request.Cursor.Value) > 0);

        var categories = await query
            .OrderBy(p => p.Id)
            .Take(pageSize + 1)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Slug, c.DisplayOrder))
            .ToListAsync(cancellationToken);

        var count = categories.Count;
        var hasNextPage = count > pageSize;
        if (hasNextPage)
            categories.RemoveAt(count - 1);

        int? nextCursor = hasNextPage ? categories[^1].Id : null;

        var response = new CursorPagedResponse<CategoryResponse, int?>(
            Items: categories,
            NextCursor: nextCursor,
            HasNextPage: hasNextPage
        );

        return ApiResponse<CursorPagedResponse<CategoryResponse, int?>>.Success(response);
    }
}
