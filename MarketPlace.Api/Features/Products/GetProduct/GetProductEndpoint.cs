using MarketPlace.Api.Common.ApiResponseFactory;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Products.GetProduct;

public static class GetProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/{productId:guid}",
                async (
                    [FromRoute] Guid productId,
                    [FromServices] GetProductHandler handler,
                    CancellationToken ct
                ) => (await handler.HandleAsync(productId, ct)).ToMinimalApiResult()
            )
            .Produces<GetProductResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
