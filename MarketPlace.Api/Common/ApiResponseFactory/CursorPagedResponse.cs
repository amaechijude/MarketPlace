namespace MarketPlace.Api.Common.ApiResponseFactory;

public sealed record CursorPagedResponse<TData, TCursor>(
    IReadOnlyCollection<TData> Items,
    TCursor? NextCursor,
    bool HasNextPage,
    int Count
);
