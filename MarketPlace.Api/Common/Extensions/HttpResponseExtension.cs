using MarketPlace.Api.Infrastucture.AuthInfrastructure;

namespace MarketPlace.Api.Common.Extensions;

public static class HttpResponseExtension
{
    extension(HttpResponse response)
    {
        public void AttachAccessToken(
            string accessToken,
            DateTimeOffset expiresOn,
            IHostEnvironment environment
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

        public void AttachRetryAfterHeader(double seconds) =>
            response.Headers.TryAdd("x-rate-limit-retry-after", $"{seconds} seconds");
    }
}
