using MarketPlace.Api.Infrastucture.Auth;

namespace MarketPlace.Api.Common.Extensions;

public static class HttpContextExtension
{
    public static void Login(
        this HttpContext httpContext,
        string accessToken,
        TimeSpan ttl,
        IWebHostEnvironment environment
    )
    {
        httpContext.Response.Cookies.Append(
            key: AuthSessionOptions.CookieKey,
            value: accessToken,
            options: new CookieOptions
            {
                HttpOnly = true,
                Secure = environment.IsProduction(),
                SameSite = environment.IsProduction() ? SameSiteMode.Strict : SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.Add(ttl),
            }
        );
    }
}
