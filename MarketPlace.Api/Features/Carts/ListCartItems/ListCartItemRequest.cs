namespace MarketPlace.Api.Features.Carts.ListCartItems;

public sealed record ListCartItemRequest(int PageSize = 30, Guid? Cursor = null);
