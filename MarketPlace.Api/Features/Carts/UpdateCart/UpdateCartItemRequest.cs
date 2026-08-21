using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Carts.UpdateCart;

public sealed record UpdateCartItemRequest([IsValidGuid] Guid ProductId, int CurrentQuantity);
