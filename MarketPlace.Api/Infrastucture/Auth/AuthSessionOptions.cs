using Microsoft.AspNetCore.Authentication;

namespace MarketPlace.Api.Infrastucture.Auth;

public sealed class CustomAuthSchemeOptions : AuthenticationSchemeOptions
{
    public const string DefaultAuthenticationScheme = "Cookie";
    public const string CookieKey = "auth_token";

    public static readonly TimeSpan DefaultTimeSpan = TimeSpan.FromDays(7);
}
