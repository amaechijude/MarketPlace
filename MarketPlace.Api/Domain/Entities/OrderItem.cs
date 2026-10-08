namespace MarketPlace.Api.Domain.Entities;

public sealed class OrderItem
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();

    // Snapshot the name,sku,price at time of order — product may change later
    public required string ProductName { get; init; }
    public required string Sku { get; init; }
    public required int Quantity { get; init; }
    public required long UnitPriceInKobo { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public MarketPlace.Api.Domain.Entities.Enums.FulfillmentStatus FulfillmentStatus { get; set; } =
        MarketPlace.Api.Domain.Entities.Enums.FulfillmentStatus.Pending;

    // Navigation
    public required Guid VendorId { get; init; }
    public Vendor Vendor { get; set; } = null!;
    public required Guid OrderId { get; init; }
    public Order Order { get; private set; } = null!;

    public required Guid ProductVariantId { get; init; }
    public ProductVariant? ProductVariant { get; set; }
}
