namespace MarketPlace.Api.Domain.Entities;

public sealed class WishlistItem
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();
    public required Guid UserId { get; init; }
    public User User { get; set; } = null!;

    public required Guid ProductId { get; init; }
    public Product Product { get; set; } = null!;

    public required DateTimeOffset CreatedAt { get; init; }
}
