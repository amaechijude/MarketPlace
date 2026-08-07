using Microsoft.AspNetCore.Authentication;

namespace MarketPlace.Api.Infrastucture.Auth;

public sealed class AuthSessionOptions : AuthenticationSchemeOptions
{
    public const string DefaultAuthenticationScheme = "Bearer";

    public static string? ExtractToken(HttpRequest request)
    {
        if (
            request.Cookies.TryGetValue("session", out var token)
            && !string.IsNullOrWhiteSpace(token)
        )
            return token;

        var header = request.Headers.Authorization.ToString();
        var length = DefaultAuthenticationScheme.Length;
        if (string.IsNullOrWhiteSpace(header) || header.Length < length + 2)
            return null;

        return header.StartsWith(DefaultAuthenticationScheme, StringComparison.OrdinalIgnoreCase)
            ? header[length..].Trim()
            : null;
    }
}
