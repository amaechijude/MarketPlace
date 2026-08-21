using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Carts.AddToCart;

public sealed class AddToCartHandler(AppDbContext context) : IRequestHandler
{
    public async Task<ApiResponse<int?>> HandleAsync(
        Guid userId,
        AddToCartRequest request,
        CancellationToken cancellationToken
    )
    {
        var product = await context
            .Products.AsNoTracking()
            .Where(p => p.Id == request.ProductId)
            .Select(s => new { s.Id, s.StockQuantity })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            return ApiResponse<int?>.BadRequest("Item no no longer exists");

        var quantity = Math.Clamp(request.Quantity, 1, 50);

        if (product.StockQuantity < quantity)
            return ApiResponse<int?>.BadRequest("Insufficient stock quantity");

        var userCart = await context
            .Carts.Include(c => c.CartItems)
            .Where(c => c.UserId == userId)
            .Select(s => new { s.Id, ItemsId = s.CartItems.Select(t => t.ProductId).ToList() })
            .FirstOrDefaultAsync(cancellationToken);

        var canSave = false;
        if (userCart is null)
        {
            var cart = Cart.Create(userId);
            cart.AddCartItem(product.Id, quantity);
            context.Carts.Add(cart);
            canSave = true;
        }
        else
        {
            if (!userCart.ItemsId.Contains(product.Id))
            {
                context.CartItems.Add(CartItem.Create(userCart.Id, product.Id, quantity));
                canSave = true;
            }
        }

        if (canSave)
            await context.SaveChangesAsync(cancellationToken);

        return ApiResponse<int?>.Created();
    }
}
