namespace MarketPlace.Api.Domain.Entities;

public sealed class Vendor
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string BannerUrl { get; set; } = string.Empty;
    public required string Location { get; set; }
    public bool IsActivated { get; set; } = false;

    public required DateTimeOffset DateJoined { get; init; }
    public DateTimeOffset? LastUpdatedAt { get; private set; }

    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = [];

    public void Update()
    {
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }
}
