using JetBrains.Annotations;

namespace MarketPlace.Api.Common.ApiResponseFactory;

public sealed record CursorPagedResponse<T>(List<T> Items, Guid? NextCursor, bool HasNextPage)
{
    [UsedImplicitly]
    public int Count => Items.Count;
};
