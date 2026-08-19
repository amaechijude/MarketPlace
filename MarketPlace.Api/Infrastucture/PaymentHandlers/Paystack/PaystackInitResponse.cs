using System.Text.Json.Serialization;

namespace MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;

/// <summary>Top-level response from Paystack's <c>/transaction/initialize</c> endpoint.</summary>
public sealed record PaystackInitResponse
{
    [JsonPropertyName("status")]
    public bool Status { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("data")]
    public required PaystackInitData Data { get; init; }
}

public sealed record PaystackInitData
{
    [JsonPropertyName("authorization_url")]
    public required string AuthorizationUrl { get; init; }

    [JsonPropertyName("access_code")]
    public required string AccessCode { get; init; }

    [JsonPropertyName("reference")]
    public required string Reference { get; init; }
}
