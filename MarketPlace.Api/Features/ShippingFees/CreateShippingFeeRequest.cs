using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Features.ShippingFees;

public sealed record CreateShippingFeeRequest(
    [Required, MinLength(3), MaxLength(50)] string StateName,
    [Range(100, int.MaxValue)] int FeeInNaira
);
