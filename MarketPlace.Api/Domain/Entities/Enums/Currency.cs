using System.Text.Json.Serialization;

namespace MarketPlace.Api.Domain.Entities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Currency
{
    NGN,
    USD,
}
