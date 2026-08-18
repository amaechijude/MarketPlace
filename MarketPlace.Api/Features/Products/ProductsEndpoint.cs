using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Products.CreateProduct;

namespace MarketPlace.Api.Features.Products;

public sealed class ProductsEndpoint : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("products").WithTags("Products");

        CreateProductEndpoint.Map(group);
    }
}
