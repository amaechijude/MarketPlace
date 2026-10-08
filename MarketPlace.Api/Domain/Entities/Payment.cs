using System.Net;
using MarketPlace.Api.Common.ExceptionHandler;
using MarketPlace.Api.Domain.Entities.Enums;

namespace MarketPlace.Api.Domain.Entities;

public sealed class Payment
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public required PaymentProvider Provider { get; init; }
    public required string Reference { get; set; } = null!;
    public required long AmountInKobo { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset? FailedAt { get; private set; }
    public DateTimeOffset? RefundedAt { get; private set; }

    // Store raw failure reason from provider for debugging
    public string? FailureReason { get; private set; }

    // Navigation
    public required Guid OrderId { get; init; }
    public Order? Order { get; private set; }

    public void MarkPaid()
    {
        Status = PaymentStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
    }
}

public sealed class PaymentOperationException(
    string message,
    HttpStatusCode statusCode = HttpStatusCode.UnprocessableContent
) : CustomAppExceptions(message, statusCode);
