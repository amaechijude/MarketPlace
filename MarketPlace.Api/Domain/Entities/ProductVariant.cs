namespace MarketPlace.Api.Domain.Entities;

public sealed class ProductVariant
{
    public Guid Id { get; private init; }

    public string Sku { get; private set; } = string.Empty;
    public long PriceInKobo { get; private set; }
    public int StockQuantity { get; set; }
    public Dictionary<string, string> Attributes { get; private set; } = [];

    // audit
    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? LastUpdatedAt { get; private set; }
    public Guid? LastUpdatedBy { get; private set; }

    // product
    public Guid ProductId { get; private init; }
    public Product Product { get; set; } = null!;

    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; private set; } = [];

    public static ProductVariant Create(
        Guid productId,
        int priceInKobo,
        DateTimeOffset createdAt,
        int quantity,
        Guid createdBy,
        Dictionary<string, string> attributes
    ) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Sku = GenerateSku(),
            PriceInKobo = priceInKobo,
            StockQuantity = quantity,
            Attributes = attributes,
            ProductId = productId,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
        };

    private static string GenerateSku()
    {
        return "";
    }
}
