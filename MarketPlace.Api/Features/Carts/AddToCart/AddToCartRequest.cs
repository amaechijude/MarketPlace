using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Carts.AddToCart;

public sealed record AddToCartRequest(
    [IsValidGuid] Guid ProductVariantId,
    [Range(1, 50)] int Quantity = 1
);
