using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Auth.Login;

public sealed record EmailLoginRequest(
    [Required, NoInBetweenWhiteSpace, EmailAddress] string Email,
    [Required, IsValidPassword] string Password
);
