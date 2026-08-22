using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.ShippingAddresses.CreateShippingAddress;
using MarketPlace.Api.Features.ShippingAddresses.DeleteShippingAddress;
using MarketPlace.Api.Features.ShippingAddresses.GetShippingAddress;
using MarketPlace.Api.Features.ShippingAddresses.ListShippingAddress;

namespace MarketPlace.Api.Features.ShippingAddresses;

public class ShippingAddressEndpoint : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("address").RequireAuthorization().WithTags("Shipping Address");

        CreateAddressEndpoint.Map(group);
        DeleteAddressEndpoint.Map(group);
        GetAddressEndpoint.Map(group);
        ListAddressEndpoint.Map(group);
    }
}
