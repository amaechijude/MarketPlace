using JetBrains.Annotations;

namespace MarketPlace.Api.Features.Products.ListProduct;

public sealed record ListProductResponse(
    Guid Id,
    string Name,
    long PriceInKobo,
    string ThumbnailUrl,
    string CategorySlug
)
{
    [UsedImplicitly]
    public long PriceInNaira => PriceInKobo / 100;
};
