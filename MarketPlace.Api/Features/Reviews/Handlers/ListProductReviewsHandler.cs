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

        var hasNextPage = reviews.Count > pageSize;
        if (hasNextPage)
        {
            reviews.RemoveAt(reviews.Count - 1);
        }

        Guid? nextCursor = hasNextPage ? reviews[^1].Id : null;

        return ApiResponse<CursorPagedResponse<ReviewResponse, Guid?>>.Success(
            new CursorPagedResponse<ReviewResponse, Guid?>(reviews, nextCursor, hasNextPage)
        );
    }
}
