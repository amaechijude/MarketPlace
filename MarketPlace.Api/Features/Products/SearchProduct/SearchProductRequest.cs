namespace MarketPlace.Api.Features.Products.SearchProduct;

public sealed record SearchProductRequest(string Query, int Limit = 10);
