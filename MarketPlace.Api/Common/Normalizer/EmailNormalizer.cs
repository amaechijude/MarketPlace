using MarketPlace.Api.Common.ExceptionHandler;

namespace MarketPlace.Api.Common.Normalizer;

public static class EmailNormalizer
{
    public static string Normalize(string email)
    {
        email = email.Trim();
        if (email.Contains(' '))
            throw new EmailWhiteSpaceException();

        return !email.Contains('@')
            ? throw new EmailWhiteSpaceException("Invaliid Email")
            : email.ToLowerInvariant();
    }
}

public sealed class EmailWhiteSpaceException(string message = "Email cannot contain whitespace")
    : CustomAppExceptions(message);
