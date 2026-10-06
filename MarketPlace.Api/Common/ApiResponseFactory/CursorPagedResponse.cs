using JetBrains.Annotations;
using MarketPlace.Api.Features.ShippingAddresses.ListShippingAddress;

namespace MarketPlace.Api.Common.ApiResponseFactory;

public sealed record CursorPagedResponse<TData, TCursor>(
    IReadOnlyCollection<TData> Items,
    TCursor? NextCursor,
    bool HasNextPage
)
{
    [UsedImplicitly]
    public int Count => Items.Count;
};
