using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Users.Register;

public sealed record RegisterUserRequest(
    [Required, NoInBetweenWhiteSpace, EmailAddress] string Email,
    string Password
);
