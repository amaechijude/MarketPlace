using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Wishlists.RemoveProduct;

public sealed record RemoveProductFromWishlistRequest(Guid ProductId);

public sealed record RemoveProductFromWishlistResponse(string Message);

public sealed class RemoveProductFromWishlistHandler(AppDbContext dbContext) : IScopedRequestHandler
{
    public async Task<ApiResponse<RemoveProductFromWishlistResponse>> HandleAsync(
        RemoveProductFromWishlistRequest request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default
    )
    {
        var userId = user.UserId;

        var existingItem = await dbContext.WishlistItems.FirstOrDefaultAsync(
            x => x.UserId == userId && x.ProductId == request.ProductId,
            cancellationToken
        );

        if (existingItem is null)
        {
            return ApiResponse<RemoveProductFromWishlistResponse>.NotFound(
                "Product not found in wishlist"
            );
        }

        dbContext.WishlistItems.Remove(existingItem);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<RemoveProductFromWishlistResponse>.Success(
            new RemoveProductFromWishlistResponse("Product removed from wishlist")
        );
    }
}
