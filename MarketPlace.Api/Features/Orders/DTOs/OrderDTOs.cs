using MarketPlace.Api.Domain.Entities.Enums;

namespace MarketPlace.Api.Features.Orders.DTOs;

public sealed record OrderItemResponse(
    Guid Id,
    string ProductName,
    string Sku,
    int Quantity,
    long UnitPriceInKobo
);

public sealed record OrderResponse(
    Guid Id,
    OrderStatus Status,
    long SubtotalInKobo,
    long ShippingFeeInKobo,
    long TotalInKobo,
    string TrackingNumber,
    string PaymentReference,
    DateTimeOffset CreatedAt,
    IEnumerable<OrderItemResponse> Items
);

public sealed record UpdateOrderStatusRequest(OrderStatus Status, string? TrackingNumber);
