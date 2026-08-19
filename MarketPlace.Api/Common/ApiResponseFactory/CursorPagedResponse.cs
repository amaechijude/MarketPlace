namespace MarketPlace.Api.Common.ApiResponseFactory;

public sealed record CursorPagedResponse<T>(List<T> Items, Guid? NextCursor, bool HasNextPage)
{
    public readonly int Count = Items.Count;
};
