using JetBrains.Annotations;

namespace MarketPlace.Api.Features.Carts.ListCartItems;

public sealed record CartItemResponse(
    Guid Id,
    string ProductName,
    Guid ProductVariantId,
    string ImageUrl,
    int Quantity,
    string CategorySlug,
    long UnitPriceInKobo
)
{
    [UsedImplicitly]
    public long UnitPriceInNaira => UnitPriceInKobo / 100;
}
