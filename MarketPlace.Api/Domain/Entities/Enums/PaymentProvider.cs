using System.Text.Json.Serialization;

namespace MarketPlace.Api.Domain.Entities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentProvider
{
    Paystack,
    Stripe,
    Flutterwave,
    ErcasPay,
    Monnify,
    Opay,
}
