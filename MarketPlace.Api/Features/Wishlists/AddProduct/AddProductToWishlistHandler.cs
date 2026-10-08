using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Wishlists.AddProduct;

public sealed record AddProductToWishlistRequest(Guid ProductId);

public sealed record AddProductToWishlistResponse(
    Guid Id,
    Guid ProductId,
    DateTimeOffset CreatedAt
);

public sealed class AddProductToWishlistHandler(AppDbContext dbContext, TimeProvider timeProvider)
    : IScopedRequestHandler
{
    public async Task<ApiResponse<AddProductToWishlistResponse>> HandleAsync(
        AddProductToWishlistRequest request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default
    )
    {
        var userId = user.UserId;

        var productExists = await dbContext.Products.AnyAsync(
            x => x.Id == request.ProductId,
            cancellationToken
        );

        if (!productExists)
        {
            return ApiResponse<AddProductToWishlistResponse>.NotFound("Product not found");
        }

        var existingItem = await dbContext.WishlistItems.FirstOrDefaultAsync(
            x => x.UserId == userId && x.ProductId == request.ProductId,
            cancellationToken
        );

        if (existingItem is not null)
        {
            return ApiResponse<AddProductToWishlistResponse>.BadRequest(
                "Product is already in the wishlist"
            );
        }

        var item = new WishlistItem
        {
            UserId = userId,
            ProductId = request.ProductId,
            CreatedAt = timeProvider.GetUtcNow(),
        };

        dbContext.WishlistItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<AddProductToWishlistResponse>.Success(
            new AddProductToWishlistResponse(item.Id, item.ProductId, item.CreatedAt)
        );
    }
}
