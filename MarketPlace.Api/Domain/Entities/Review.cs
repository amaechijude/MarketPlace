namespace MarketPlace.Api.Domain.Entities;

public sealed class Review
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();
    public required Guid ProductId { get; init; }
    public Product Product { get; private set; } = null!;

    public required Guid UserId { get; init; }
    public User User { get; private set; } = null!;

    public required Guid VendorId { get; init; }
    public Vendor Vendor { get; private set; } = null!;

    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private init; }

    public static Review Create(
        Guid productId,
        Guid vendorId,
        Guid userId,
        int rating,
        string comment,
        DateTimeOffset createdAt
    )
    {
        return new Review
        {
            ProductId = productId,
            VendorId = vendorId,
            UserId = userId,
            Rating = Math.Clamp(rating, 1, 5),
            Comment = comment,
            CreatedAt = createdAt,
        };
    }
}
