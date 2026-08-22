using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Checkout;

public sealed record CheckoutRequest([IsValidGuid] Guid AdrressId);
