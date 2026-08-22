using System.Text.Json.Serialization;

namespace MarketPlace.Api.Domain.Entities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    Pending,
    ConfirmedPayment,
    Shipped,
    Delivered,
    Cancelled,
}
