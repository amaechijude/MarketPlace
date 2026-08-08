namespace MarketPlace.Api.Common.ApiResponseFactory;

public sealed record CursorPagedResponse<T>(
    IEnumerable<T> Items,
    Guid? NextCursor,
    int Count,
    bool HasNextPage
);
