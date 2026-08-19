namespace MarketPlace.Api.Features.Products.ListProduct;

public sealed record ListProductRequest(
    Guid? Cursor = null,
    int PageSize = 30,
    string? CategorySlug = null
);
