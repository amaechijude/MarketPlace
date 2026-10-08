using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Wishlists.GetMyWishlist;

public sealed record GetMyWishlistRequest();

public sealed record GetMyWishlistResponseItem(
    Guid ProductId,
    string ProductName,
    string ThumbnailUrl,
    long BasePriceInKobo,
    DateTimeOffset AddedAt
);

public sealed class GetMyWishlistHandler(AppDbContext dbContext) : IScopedRequestHandler
{
    public async Task<ApiResponse<IEnumerable<GetMyWishlistResponseItem>>> HandleAsync(
        GetMyWishlistRequest request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default
    )
    {
        var userId = user.UserId;

        var items = await dbContext
            .WishlistItems.AsNoTracking()
            .Include(x => x.Product)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new GetMyWishlistResponseItem(
                x.ProductId,
                x.Product.Name,
                x.Product.ThumbnailUrl,
                x.Product.BasePriceInKobo,
                x.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<IEnumerable<GetMyWishlistResponseItem>>.Success(items);
    }
}
