namespace MarketPlace.Api.Features.Categories;

public sealed record ListCategoriesRequest(int? Cursor = null, int PageSize = 30);
