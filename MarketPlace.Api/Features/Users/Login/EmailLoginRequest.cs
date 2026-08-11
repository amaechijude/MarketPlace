using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Users.Login;

public sealed record EmailLoginRequest(
    [Required, NoInBetweenWhiteSpace, EmailAddress] string Email,
    [Required, IsValidPassword] string Password
);

public record EmailLoginResponse(Guid OtpId, string Message = "Verify otp");

public sealed record EmailLoginVerifyOtpRequest(
    [IsValidGuid] Guid OtpId,
    [Required, MinLength(6)] string OtpInput
);
