using MarketPlace.Api.Common.Normalizer;

namespace MarketPlace.Api.Domain.Entities;

public sealed class Category
{
    public int Id { get; init; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Navigation
    public ICollection<Product> Products { get; private init; } = [];

    public static Category Create(string name, DateTimeOffset createdAt) =>
        // Slugify the name
        new()
        {
            Name = name,
            Slug = Slugger.Slugify(name),
            CreatedAt = createdAt,
        };
}
