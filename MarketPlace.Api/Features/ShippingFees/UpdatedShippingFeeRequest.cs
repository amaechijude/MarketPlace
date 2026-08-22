using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Features.ShippingFees;

public sealed record UpdatedShippingFeeRequest([Range(100, int.MaxValue)] int FeeInNaira);
