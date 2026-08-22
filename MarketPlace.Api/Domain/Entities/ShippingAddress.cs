namespace MarketPlace.Api.Domain.Entities;

public sealed class ShippingAddress(Guid userId)
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();
    public Guid UserId { get; init; } = userId;
    public User User { get; set; } = null!;

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Phone { get; set; }
    public required string Address { get; set; }
    public required string Landmark { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public bool IsDefault { get; private set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void MarkAsDefault() => IsDefault = true;
}
