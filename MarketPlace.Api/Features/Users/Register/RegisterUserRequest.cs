using MarketPlace.Api.Common.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Features.Users.Register;

public sealed record RegisterUserRequest(
    [Required, NoInBetweenWhiteSpace, EmailAddress] string Email,
    [Required, IsValidPassword] string Password
);

public sealed record RegisterUserResponse(
    Guid OtpId,
    string Message = "Registration Successful. Check email for otp"
);
