using MarketPlace.Api.Infrastucture.Auth;

namespace MarketPlace.Api.Common.Extensions;

public static class HttpRequestExtension
{
    public static string? ExtractToken(this HttpRequest request)
    {
        _ = request.Cookies.TryGetValue(CustomAuthSchemeOptions.CookieKey, out var accessToken);
        return accessToken;

        // var header = request.Headers.Authorization.ToString();
        // const string scheme = AuthSessionOptions.DefaultAuthenticationScheme;
        // var length = scheme.Length;

        // if (string.IsNullOrWhiteSpace(header) || header.Length < length + 2)
        //     return null;

        // return header.StartsWith(scheme, StringComparison.OrdinalIgnoreCase)
        //     ? header[length..].Trim()
        //     : null;
    }
}
