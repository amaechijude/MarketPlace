using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;

public sealed class PayStackSettings
{
    [Required, MinLength(10)]
    public string SecretKey { get; set; } = string.Empty;

    [Required, Url]
    public string BaseUrl { get; set; } = string.Empty;
}
