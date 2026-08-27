using System.Linq.Expressions;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Carts.ListCartItems;

public sealed class ListCartItemHandler(AppDbContext context) : IScopedRequestHandler
{
    public async Task<ApiResponse<CursorPagedResponse<CartItemResponse>>> HandleAsync(
        Guid userId,
        ListCartItemRequest request,
        CancellationToken cancellationToken
    )
    {
        var pageSize = Math.Clamp(request.PageSize, 10, 50);

        var query = context.CartItems.AsNoTracking().Where(ci => ci.Cart.UserId == userId);
        if (request.Cursor.HasValue)
            query = query.Where(p => p.Id.CompareTo(request.Cursor.Value) > 0);

        var cartItems = await query
            .OrderBy(q => q.Id)
            .Take(pageSize + 1)
            .Select(ProjectCartItemToResponse)
            .ToListAsync(cancellationToken);

        var count = cartItems.Count;
        var hasNextPage = count > pageSize;
        if (hasNextPage)
            cartItems.RemoveAt(count - 1);

        Guid? nextCursor = hasNextPage ? cartItems[^1].Id : null;

        var response = new CursorPagedResponse<CartItemResponse>(
            Items: cartItems,
            NextCursor: nextCursor,
            HasNextPage: hasNextPage
        );

        return ApiResponse<CursorPagedResponse<CartItemResponse>>.Success(response);
    }

    private static readonly Expression<Func<CartItem, CartItemResponse>> ProjectCartItemToResponse =
        s => new CartItemResponse(
            Id: s.Id,
            ProductName: s.ProductVariant.Product.Name,
            ProductVariantId: s.ProductVariantId,
            ImageUrl: s.ProductVariant.Product.ThumbnailUrl,
            Quantity: s.Quantity,
            CategorySlug: s.ProductVariant.Product.Category.Name,
            UnitPriceInKobo: s.ProductVariant.PriceInKobo
        );
}
