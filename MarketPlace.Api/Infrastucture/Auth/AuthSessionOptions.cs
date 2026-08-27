using Microsoft.AspNetCore.Authentication;

namespace MarketPlace.Api.Infrastucture.Auth;

public sealed class AuthSessionOptions : AuthenticationSchemeOptions
{
    public const string DefaultAuthenticationScheme = "Cookie";
    public const string CookieKey = "auth_token";

    public static TimeSpan DefaultTimeSpan => TimeSpan.FromDays(7);
}
