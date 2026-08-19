using System.Text.Json.Serialization;

namespace MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;

/// <summary>Payload sent to Paystack to initialise a new transaction.</summary>
public sealed record PaystackInitPayload
{
    [JsonPropertyName("email")]
    public required string Email { get; init; }

    /// <summary>Amount in the smallest currency unit (e.g. kobo for NGN).</summary>
    [JsonPropertyName("amount")]
    public required string AmountInKobo { get; init; }

    [JsonPropertyName("reference")]
    public required string Reference { get; init; }

    [JsonPropertyName("channels")]
    public string[] Channels { get; init; } = ["bank", "card", "ussd"];

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "NGN";
}
