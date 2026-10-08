using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Auth.Register;

public sealed record RegisterUserVerifyOtpRequest(
    [IsValidGuid] Guid OtpId,
    [Required, MinLength(6)] string OtpInput
);
