using System.Text.Json.Serialization;

namespace MarketPlace.Api.Infrastucture.Email.Sendly;

public sealed record SendlyEmailRequest
{
    /// <summary>
    /// Collection of Email reciepients address
    /// </summary>
    [JsonPropertyName("to")]
    public required string[] ToEmail { get; init; }

    [JsonPropertyName("from")]
    public required string From { get; init; } = string.Empty;

    [JsonPropertyName("subject")]
    public required string Subject { get; init; }

    [JsonPropertyName("html")]
    public required string HtmlBody { get; init; }
}
