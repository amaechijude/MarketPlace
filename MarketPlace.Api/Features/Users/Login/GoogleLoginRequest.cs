using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Users.Login;

public sealed record GoogleLoginRequest([Required, IsValidJwt] string Jwt);
