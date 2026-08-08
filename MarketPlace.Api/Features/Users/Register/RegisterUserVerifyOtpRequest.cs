using MarketPlace.Api.Common.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Features.Users.Register;

public sealed record RegisterUserVerifyOtpRequest(
    [IsValidGuid] Guid OtpId,
    [Required, MinLength(6)] string OtpInput
);
