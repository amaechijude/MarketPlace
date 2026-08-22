namespace MarketPlace.Api.Domain.Entities;

public sealed class OrderItem
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();

    // Snapshot the name,sku,price at time of order — product may change later
    public required string ProductName { get; init; }
    public required string Sku { get; init; }
    public required int Quantity { get; init; }
    public required long UnitPriceInKobo { get; init; }

    // Navigation
    public Guid OrderId { get; set; }
    public Order Order { get; private set; } = null!;

    public required Guid ProductId { get; init; }
    public Product? Product { get; set; }
}
