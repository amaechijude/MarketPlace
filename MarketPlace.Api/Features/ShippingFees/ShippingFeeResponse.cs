using JetBrains.Annotations;

namespace MarketPlace.Api.Features.ShippingFees;

public sealed record ShippingFeeResponse(int Id, string Name, int FeeInKobo)
{
    [UsedImplicitly]
    public int FeeInNaira => FeeInKobo / 100;
};
