using MarketPlace.Api.Domain.Entities.Enums;
using MarketPlace.Api.Domain.Entities.OwnedTypes;

namespace MarketPlace.Api.Domain.Entities;

public sealed class Order
{
    public required Guid Id { get; init; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    // price snapshot
    public long SubtotalInKobo { get; private set; }
    public required long ShippingFeeInKobo { get; init; }
    public long TotalInKobo { get; private set; }

    // snapshot of shipping address at time of order
    public ShippingAddressSnapshot ShippingAddressSnapshot { get; private init; } = null!;

    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Navigation
    public required Guid UserId { get; init; }
    public User? User { get; set; }
    public ICollection<OrderItem> OrderItems { get; private init; } = [];

    public string TrackingNumber { get; private set; } = string.Empty;
    public required string PaymentReference { get; init; }

    public long AttachSubTotal(long subtotalAmountInKobo)
    {
        var sum = subtotalAmountInKobo + ShippingFeeInKobo;
        SubtotalInKobo = subtotalAmountInKobo;
        TotalInKobo = sum;
        return sum;
    }
}
