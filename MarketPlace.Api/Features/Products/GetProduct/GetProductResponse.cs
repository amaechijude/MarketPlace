namespace MarketPlace.Api.Features.Products.GetProduct;

public sealed record GetProductResponse(
    Guid Id,
    string Name,
    long PriceInKobo,
    string LongDescription,
    string ShortDescription,
    string ThumbnailUrl,
    string[] ImageUrlsArray,
    string CategorySlug
)
{
    public long PriceInNaira => PriceInKobo / 100;
}
