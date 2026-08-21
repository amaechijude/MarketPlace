using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Carts.SyncCartItems;

public sealed class SyncCartItemsHandler(AppDbContext context)
{
    public async Task<ApiResponse<int?>> SyncCartItemsAsync(
        Guid userId,
        SyncCartItemRequest request,
        CancellationToken cancellationToken
    )
    {
        if (request.CLientItems is { Count: 0 })
            return ApiResponse<int?>.NoContent();

        // unique variant Ids
        var products = request.CLientItems.Select(s => new { s.ProductId, s.Quantity }).ToList();

        var pIds = products.Select(s => s.ProductId).ToHashSet();
        if (pIds is { Count: 0 })
            return ApiResponse<int?>.NoContent();

        // check if variants exist in the db
        var existingProductIds = await context
            .Products.AsNoTracking()
            .Where(pv => pIds.Contains(pv.Id))
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (existingProductIds is { Count: 0 })
            return ApiResponse<int?>.NoContent();

        var safeSyncRequest = products
            .Where(s => existingProductIds.Contains(s.ProductId))
            .ToList();

        if (safeSyncRequest is { Count: 0 })
            return ApiResponse<int?>.NoContent();

        var userCart = await context
            .Carts.Where(c => c.UserId == userId)
            .Select(s => new { s.Id, ItemsId = s.CartItems.Select(i => i.ProductId).ToList() })
            .FirstOrDefaultAsync(cancellationToken);

        var canSave = false;
        if (userCart is null)
        {
            var cart = Cart.Create(userId);
            cart.AddCartItem(safeSyncRequest.Select(s => (s.ProductId, s.Quantity)));
            context.Carts.Add(cart);
            canSave = true;
        }
        else
        {
            var newItems = safeSyncRequest
                .Where(r => !userCart.ItemsId.Contains(r.ProductId))
                .ToList();
            if (newItems is { Count: > 0 })
            {
                var itesm = newItems.Select(s =>
                    CartItem.Create(userCart.Id, s.ProductId, s.Quantity)
                );
                context.CartItems.AddRange(itesm);
                canSave = true;
            }
        }

        if (canSave)
            await context.SaveChangesAsync(cancellationToken);

        return ApiResponse<int?>.NoContent();
    }
}
