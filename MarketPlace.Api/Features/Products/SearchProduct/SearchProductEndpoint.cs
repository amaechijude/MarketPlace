using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Features.Products.ListProduct;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Products.SearchProduct;

public static class SearchProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/search",
                async (
                    [AsParameters] SearchProductRequest request,
                    [FromServices] SearchProductHandler handler,
                    CancellationToken ct
                ) => (await handler.HandleAsync(request, ct)).ToMinimalApiResult()
            )
            .Produces<CursorPagedResponse<ListProductResponse, Guid?>>();
    }
}
