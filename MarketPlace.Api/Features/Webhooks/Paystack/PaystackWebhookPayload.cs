using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarketPlace.Api.Features.Webhooks.Paystack;

public sealed record PaystackWebhookPayload
{
    [JsonPropertyName("event")]
    public string Event { get; init; } = string.Empty;

    [JsonPropertyName("data")]
    public JsonElement Data { get; init; }
}
