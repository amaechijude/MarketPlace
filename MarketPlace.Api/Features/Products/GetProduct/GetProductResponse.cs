namespace MarketPlace.Api.Features.Products.GetProduct;

public sealed record GetProductResponse(
    Guid Id,
    string Name,
    string LongDescription,
    string ShortDescription,
    string ThumbnailUrl,
    string[] ImageUrlsArray,
    string CategorySlug,
    List<VariantResponse> Variants
);

public sealed record VariantResponse(
    Guid Id,
    string Sku,
    long PriceInKobo,
    int StockQuantity,
    Dictionary<string, string> Attributes
)
{
    public long PriceInNaira => PriceInKobo / 100;
};
