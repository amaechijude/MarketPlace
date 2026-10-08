using MarketPlace.Api.Common.ExceptionHandler;
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

    // Discount
    public Guid? CouponId { get; init; }
    public Coupon? Coupon { get; set; }
    public long DiscountAmountInKobo { get; private set; }

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

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public void UpdateStatus(OrderStatus status, string? trackingNumber, DateTimeOffset updatedAt)
    {
        if (status < Status)
            throw new OrderStatusTransitionException(Status, status);

        Status = status;
        if (!string.IsNullOrWhiteSpace(trackingNumber))
            TrackingNumber = trackingNumber;
        UpdatedAt = updatedAt;
    }

    public long AttachSubTotal(long subtotalAmountInKobo, long discountAmountInKobo = 0)
    {
        DiscountAmountInKobo = discountAmountInKobo;
        var sum = subtotalAmountInKobo + ShippingFeeInKobo - discountAmountInKobo;
        if (sum < 0)
            sum = 0;

        SubtotalInKobo = subtotalAmountInKobo;
        TotalInKobo = sum;
        return sum;
    }

    private void TransitionStatus(OrderStatus targetStatus)
    {
        var isValidTransition = (Status, targetStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.ConfirmedPayment) => true,
            (OrderStatus.ConfirmedPayment, OrderStatus.Shipped) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            _ => false,
        };

        if (!isValidTransition)
            throw new OrderStatusTransitionException(Status, targetStatus);

        var oldStatus = Status;
        Status = targetStatus;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, targetStatus));
    }
}

public sealed class OrderStatusTransitionException(OrderStatus currentStatus, OrderStatus newStatus)
    : CustomAppExceptions(
        $"Cannot transition order status from '{currentStatus}' to '{newStatus}'"
    );

public interface IDomainEvent;

public sealed record OrderStatusChangedEvent(
    Guid OrderId,
    OrderStatus OldStatus,
    OrderStatus NewStatus
) : IDomainEvent;
