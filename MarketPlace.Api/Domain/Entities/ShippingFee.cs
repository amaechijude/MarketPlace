using MarketPlace.Api.Common.Normalizer;

namespace MarketPlace.Api.Domain.Entities;

public sealed class ShippingFee
{
    public int Id { get; init; }

    public string StateName { get; private set; } = string.Empty;
    public string NormalizedStateName { get; private set; } = string.Empty;
    public int FeeInKobo { get; private set; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }

    public Guid? LastUpdatedById { get; private set; }
    public DateTimeOffset? LastUpdatedAt { get; private set; }

    public static ShippingFee Create(Guid createdBy, string stateName, int feeInKobo) =>
        new()
        {
            StateName = stateName,
            NormalizedStateName = StateNameNormalizer.Normalize(stateName),
            FeeInKobo = feeInKobo,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
        };

    public void UpdateName(string newName, Guid updatedById)
    {
        LastUpdatedById = updatedById;
        StateName = newName;
        NormalizedStateName = StateNameNormalizer.Normalize(newName);
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateFee(int newFeeInNaira, Guid updatedById)
    {
        LastUpdatedById = updatedById;
        FeeInKobo = newFeeInNaira * 100;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        StateName = StateName.Trim().ToLower();
    }
}
