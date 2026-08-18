using Microsoft.AspNetCore.Authentication;

namespace MarketPlace.Api.Infrastucture.Auth;

public sealed class AuthSessionOptions : AuthenticationSchemeOptions
{
    public const string DefaultAuthenticationScheme = "Bearer";
    public const string CookieKey = "auth_token";
}
