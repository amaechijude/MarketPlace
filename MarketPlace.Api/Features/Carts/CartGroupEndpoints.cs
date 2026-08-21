using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Carts.AddToCart;
using MarketPlace.Api.Features.Carts.ListCartItems;
using MarketPlace.Api.Features.Carts.RemoveFromCart;
using MarketPlace.Api.Features.Carts.SyncCartItems;
using MarketPlace.Api.Features.Carts.UpdateCart;

namespace MarketPlace.Api.Features.Carts;

public sealed class CartEndpoints : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("cart").WithTags("Cart").RequireAuthorization();

        AddToCartEndpoint.Map(group);
        ListCartItemsEndpoint.Map(group);
        RemoveItemFromCartEndpoint.Map(group);
        SynCcartItemsEndpoint.Map(group);
        UpdateItemQuantityEndpoint.Map(group);
    }
}
