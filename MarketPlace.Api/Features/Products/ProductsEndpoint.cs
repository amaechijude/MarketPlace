using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Products.CreateProduct;
using MarketPlace.Api.Features.Products.DeleteProduct;
using MarketPlace.Api.Features.Products.GetProduct;
using MarketPlace.Api.Features.Products.ListProduct;
using MarketPlace.Api.Features.Products.UpdateProduct;

namespace MarketPlace.Api.Features.Products;

public sealed class ProductsEndpoint : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("products").WithTags("Products");

        CreateProductEndpoint.Map(group);
        DeleteProductEndpoint.Map(group);
        UpdateProductEndpoint.Map(group);
        GetProductEndpoint.Map(group);
        ListProductEndpoint.Map(group);
    }
}
