using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Auth;

public sealed record VerifyOtpApiRequest(
    [property: Required, EmailAddress, NoInBetweenWhiteSpace] string Email,
    [property: Required, MinLength(6)] string OtpInput,
    bool RememberMe = false
);
