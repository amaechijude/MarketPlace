using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Wishlists.AddProduct;
using MarketPlace.Api.Features.Wishlists.GetMyWishlist;
using MarketPlace.Api.Features.Wishlists.RemoveProduct;

namespace MarketPlace.Api.Features.Wishlists;

public sealed class WishlistsEndpoint : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("wishlists").WithTags("Wishlists");
        var authGroup = group.MapGroup("").RequireAuthorization();

        AddProductToWishlistEndpoint.Map(authGroup);
        RemoveProductFromWishlistEndpoint.Map(authGroup);
        GetMyWishlistEndpoint.Map(authGroup);
    }
}
