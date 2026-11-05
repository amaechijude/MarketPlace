using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.Reviews.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Reviews.Handlers;

public sealed class ListProductReviewsHandler(AppDbContext context) : IScopedRequestHandler
{
    public async Task<ApiResponse<CursorPagedResponse<ReviewResponse, Guid?>>> HandleAsync(
        Guid productId,
        Guid? cursor,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        pageSize = Math.Clamp(pageSize, 10, 50);

        var query = context
            .Reviews.AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.ProductId == productId);

        if (cursor.HasValue)
        {
            query = query.Where(r => r.Id.CompareTo(cursor.Value) > 0);
        }

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(pageSize + 1)
            .Select(r => new ReviewResponse(r.Id, r.Rating, r.Comment, r.CreatedAt, r.User.Email))
            .ToListAsync(cancellationToken);

        var count = reviews.Count;
        var hasNextPage = count > pageSize;
        Guid? nextCursor = null;

        if (hasNextPage)
        {
            nextCursor = reviews[^1].Id;
            reviews.RemoveAt(count - 1);
        }

        var response = new CursorPagedResponse<ReviewResponse, Guid?>(
            Items: reviews,
            NextCursor: nextCursor,
            HasNextPage: hasNextPage,
            Count: count
        );

        return ApiResponse<CursorPagedResponse<ReviewResponse, Guid?>>.Success(response);
    }
}
