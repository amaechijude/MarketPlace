namespace MarketPlace.Api.Domain.Entities;

public sealed class Wallet
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();
    public required Guid VendorId { get; init; }
    public Vendor Vendor { get; set; } = null!;

    public long AvailableBalanceInKobo { get; private set; }
    public long PendingBalanceInKobo { get; private set; }

    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<WalletTransaction> Transactions { get; set; } = [];
    public ICollection<PayoutRequest> Payouts { get; set; } = [];

    public void CreditPending(long amountInKobo, DateTimeOffset updatedAt)
    {
        PendingBalanceInKobo += amountInKobo;
        UpdatedAt = updatedAt;
    }

    public void MovePendingToAvailable(long amountInKobo, DateTimeOffset updatedAt)
    {
        PendingBalanceInKobo -= amountInKobo;
        AvailableBalanceInKobo += amountInKobo;
        UpdatedAt = updatedAt;
    }

    public void DebitAvailable(long amountInKobo, DateTimeOffset updatedAt)
    {
        AvailableBalanceInKobo -= amountInKobo;
        UpdatedAt = updatedAt;
    }
}
