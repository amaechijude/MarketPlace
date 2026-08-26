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
        var variants = request
            .CLientItems.Select(s => new { s.ProductVariantId, s.Quantity })
            .ToList();

        var variantIds = variants.Select(s => s.ProductVariantId).ToHashSet();
        if (variantIds is { Count: 0 })
            return ApiResponse<int?>.NoContent();

        // check if variants exist in the db
        var existingVariantIds = await context
            .ProductVariants.AsNoTracking()
            .Where(pv => variantIds.Contains(pv.Id))
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (existingVariantIds is { Count: 0 })
            return ApiResponse<int?>.NoContent();

        var safeSyncRequest = variants
            .Where(s => existingVariantIds.Contains(s.ProductVariantId))
            .ToList();

        if (safeSyncRequest is { Count: 0 })
            return ApiResponse<int?>.NoContent();

        var userCart = await context
            .Carts.Where(c => c.UserId == userId)
            .Select(s => new
            {
                s.Id,
                ItemsId = s.CartItems.Select(i => i.ProductVariantId).ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var canSave = false;
        if (userCart is null)
        {
            var cart = Cart.Create(userId);
            cart.AddCartItem(safeSyncRequest.Select(s => (s.ProductVariantId, s.Quantity)));
            context.Carts.Add(cart);
            canSave = true;
        }
        else
        {
            var newItems = safeSyncRequest
                .Where(r => !userCart.ItemsId.Contains(r.ProductVariantId))
                .ToList();
            if (newItems is { Count: > 0 })
            {
                var itesm = newItems.Select(s =>
                    CartItem.Create(userCart.Id, s.ProductVariantId, s.Quantity)
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
