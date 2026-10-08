using MarketPlace.Api.Domain.Entities.Enums;

namespace MarketPlace.Api.Domain.Entities;

public sealed class PayoutRequest
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public required Guid WalletId { get; init; }
    public Wallet Wallet { get; set; } = null!;

    public required long AmountInKobo { get; init; }
    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;

    public string? BankAccountName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }

    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public Guid? ProcessedBy { get; set; }
    public string? RejectionReason { get; set; }
}
