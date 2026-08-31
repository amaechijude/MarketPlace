using MarketPlace.Api.Infrastucture.Auth;

namespace MarketPlace.Api.Common.Extensions;

public static class HttpResponseExtension
{
    public static void AttachAccessToken(
        this HttpResponse response,
        string accessToken,
        DateTimeOffset expiresOn,
        IWebHostEnvironment environment
    )
    {
        response.Cookies.Append(
            key: CustomAuthSchemeOptions.CookieKey,
            value: accessToken,
            options: new CookieOptions
            {
                HttpOnly = true,
                Secure = environment.IsProduction(),
                SameSite = environment.IsProduction() ? SameSiteMode.Strict : SameSiteMode.Lax,
                Expires = expiresOn,
            }
        );
    }
}
