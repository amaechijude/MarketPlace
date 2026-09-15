using MarketPlace.Api.Features.ShippingAddresses.CreateShippingAddress;

namespace MarketPlace.Api.Features.ShippingAddresses.ListShippingAddress;

public sealed record ListAddressResponse(List<AddressResponse> Addresses);
