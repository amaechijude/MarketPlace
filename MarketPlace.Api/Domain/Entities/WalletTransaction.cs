using MarketPlace.Api.Domain.Entities.Enums;

namespace MarketPlace.Api.Domain.Entities;

public sealed class WalletTransaction
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public required Guid WalletId { get; init; }
    public Wallet Wallet { get; set; } = null!;

    public required long AmountInKobo { get; init; }
    public required TransactionType Type { get; init; }

    public string? Reference { get; init; }
    public string? Description { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}
