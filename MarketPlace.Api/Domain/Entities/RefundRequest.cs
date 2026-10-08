using MarketPlace.Api.Domain.Entities.Enums;

namespace MarketPlace.Api.Domain.Entities;

public sealed class RefundRequest
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public required Guid OrderId { get; init; }
    public Order Order { get; set; } = null!;

    public Guid? OrderItemId { get; init; }
    public OrderItem? OrderItem { get; set; }

    public required Guid UserId { get; init; }
    public User User { get; set; } = null!;

    public required string Reason { get; set; }
    public required long AmountInKobo { get; init; }

    public RefundStatus Status { get; set; } = RefundStatus.Pending;

    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? RejectionReason { get; set; }
}
