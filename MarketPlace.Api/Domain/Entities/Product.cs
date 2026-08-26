using MarketPlace.Api.Features.Products.CreateProduct;

namespace MarketPlace.Api.Domain.Entities;

public sealed class Product
{
    public required Guid Id { get; init; }
    public required string Name { get; set; } = string.Empty;
    public required string ShortDescription { get; set; } = string.Empty;
    public required string LongDescription { get; set; } = string.Empty;
    public required long BasePriceInKobo { get; set; }

    // images
    public required string ThumbnailUrl { get; set; } = string.Empty;
    public required string ThumbnailFileKey { get; set; } = string.Empty;
    public required string[] ImageUrlsArray { get; set; } = [];
    public required string[] ImageFileKeysArray { get; set; } = [];

    // audit
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
    public Guid? LastUpdatedBy { get; set; }

    // query filters
    public required bool IsPublished { get; set; }
    public Category Category { get; set; } = null!;
    public required int CategoryId { get; set; }

    // rel
    public required Guid VendorId { get; init; }
    public Vendor Vendor { get; private set; } = null!;

    public ICollection<ProductVariant> Variants { get; set; } = [];

    public void AddVariant(CreateVariantRequest request, DateTimeOffset createdAt, Guid createdby)
    {
        var variant = ProductVariant.Create(
            Id,
            request.PriceInKobo,
            createdAt,
            request.Quantity,
            createdby,
            request.Attributes
        );
        Variants.Add(variant);
    }

    public void AddVariant(
        IEnumerable<CreateVariantRequest> requests,
        DateTimeOffset createdAt,
        Guid createdby
    )
    {
        foreach (var variant in requests)
        {
            AddVariant(variant, createdAt, createdby);
        }
    }
}
