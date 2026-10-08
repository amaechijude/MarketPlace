using MarketPlace.Api.Domain.Entities.Enums;

namespace MarketPlace.Api.Domain.Entities;

public sealed class Coupon
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public required string Code { get; init; }
    public required DiscountType DiscountType { get; init; }
    public required long DiscountValue { get; init; }

    public long? MinOrderAmountInKobo { get; init; }
    public long? MaxDiscountAmountInKobo { get; init; }

    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }

    public int? UsageLimit { get; init; }
    public int UsedCount { get; private set; }

    public Guid? VendorId { get; init; }
    public Vendor? Vendor { get; set; }

    public bool IsActive { get; set; } = true;

    public required DateTimeOffset CreatedAt { get; init; }

    public void IncrementUsedCount()
    {
        UsedCount++;
    }
}
