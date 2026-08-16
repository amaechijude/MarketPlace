namespace MarketPlace.Api.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long PriceInKobo { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string ThumbnailFileKey { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Stored as comma-separated or JSON in the DB column; configured via Fluent API
    public string[] ImageArray { get; set; } = [];
    public string[] ImageFileKeysArray { get; set; } = [];

    // query filters
    public bool IsPublished { get; set; } = true;
    public Guid VendorId { get; init; }
}
