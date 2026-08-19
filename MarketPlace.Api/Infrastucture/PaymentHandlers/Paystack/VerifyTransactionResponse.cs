using System.Text.Json.Serialization;

namespace MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;

public sealed record VerifyTransactionResponse
{
    [JsonPropertyName("status")]
    public bool Status { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("data")]
    public PaystackEventData? Data { get; init; }
}

public sealed record PaystackEventData
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("domain")]
    public string Domain { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("reference")]
    public string Reference { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public long AmountInKobo { get; init; }

    [JsonPropertyName("gateway_response")]
    public string GatewayResponse { get; init; } = string.Empty;

    [JsonPropertyName("paid_at")]
    public DateTime PaidAt { get; init; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("channel")]
    public string Channel { get; init; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    [JsonPropertyName("fees")]
    public int Fees { get; init; }
}
