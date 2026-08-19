using MarketPlace.Api.Common.ApiResponseFactory;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Products.ListProduct;

public static class ListProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/",
                async (
                    [AsParameters] ListProductRequest request,
                    [FromServices] ListProductHandler handler,
                    CancellationToken ct
                ) => (await handler.HandleAsync(request, ct)).ToMinimalApiResult()
            )
            .Produces<CursorPagedResponse<ListProductResponse>>();
    }
}
