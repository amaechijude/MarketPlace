using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Users.ForgotPassword;

public sealed record ForgotPasswordRequest(
    [Required, EmailAddress, NoInBetweenWhiteSpace] string Email
);
