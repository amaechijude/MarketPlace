namespace MarketPlace.Api.Features.Checkout;

public sealed record CheckoutResponse(string Reference, string AccessCode, string AuthorizationUrl);
