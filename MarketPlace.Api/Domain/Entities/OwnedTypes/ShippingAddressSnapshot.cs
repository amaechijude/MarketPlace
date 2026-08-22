namespace MarketPlace.Api.Domain.Entities.OwnedTypes;

public sealed record ShippingAddressSnapshot(
    Guid Id,
    string FirstName,
    string LastName,
    string Phone,
    string Address,
    string Landmark,
    string City,
    string State
);
