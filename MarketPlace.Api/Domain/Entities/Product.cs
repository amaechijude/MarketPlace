namespace MarketPlace.Api.Domain.Entities;

public sealed class Product
{
    public required Guid Id { get; init; }
    public required string Name { get; set; } = string.Empty;
    public required string ShortDescription { get; set; } = string.Empty;
    public required string LongDescription { get; set; } = string.Empty;
    public required long PriceInKobo { get; set; }
    public required string ThumbnailUrl { get; set; } = string.Empty;
    public required string ThumbnailFileKey { get; set; } = string.Empty;

    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
    public Guid? LastUpdatedBy { get; set; }

    // Stored as comma-separated string
    public required string[] ImageUrlsArray { get; set; } = [];
    public required string[] ImageFileKeysArray { get; set; } = [];

    // Audit
    public int StockQuantity { get; set; }

    // query filters
    public required bool IsPublished { get; set; }
    public Category Category { get; set; } = null!;
    public required int CategoryId { get; set; }

    public required Guid CreatedBy { get; init; }

    // rel
    public ICollection<CartItem> CartItems { get; set; } = [];
}
