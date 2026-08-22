using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Features.Checkout;

public sealed record ValidateCheckoutRequest([Required, MinLength(10)] string Reference);
