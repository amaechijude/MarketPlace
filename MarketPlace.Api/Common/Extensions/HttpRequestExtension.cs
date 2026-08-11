using MarketPlace.Api.Infrastucture.Auth;

namespace MarketPlace.Api.Common.Extensions;

public static class HttpRequestExtension
{
    public static string? ExtractToken(this HttpRequest request)
    {
        if (
            request.Cookies.TryGetValue(AuthSessionOptions.CookieKey, out var token)
            && !string.IsNullOrWhiteSpace(token)
        )
            return token;

        var header = request.Headers.Authorization.ToString();
        const string scheme = AuthSessionOptions.DefaultAuthenticationScheme;
        var length = scheme.Length;

        if (string.IsNullOrWhiteSpace(header) || header.Length < length + 2)
            return null;

        return header.StartsWith(scheme, StringComparison.OrdinalIgnoreCase)
            ? header[length..].Trim()
            : null;
    }
}