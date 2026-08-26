namespace MarketPlace.Api.Domain.Entities;

public sealed class Role
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreateAt { get; init; }
    public ICollection<User> Users { get; set; } = [];
    public Guid CreatedBy { get; init; } = Guid.Empty;

    public static Role Create(string name, Guid createdBy)
    {
        return new Role
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            CreatedBy = createdBy,
            CreateAt = DateTimeOffset.UtcNow,
        };
    }
}
