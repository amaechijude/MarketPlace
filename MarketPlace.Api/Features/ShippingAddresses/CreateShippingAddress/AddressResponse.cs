namespace MarketPlace.Api.Features.ShippingAddresses.CreateShippingAddress;

public sealed record AddressResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Phone,
    string Address,
    string Landmark,
    string City,
    string State
);
