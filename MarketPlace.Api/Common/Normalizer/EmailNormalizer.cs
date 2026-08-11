using MarketPlace.Api.Common.ExceptionHandler;

namespace MarketPlace.Api.Common.Normalizer;

public static class EmailNormalizer
{
    public static string Normalize(string email)
    {
        email = email.Trim();
        if (email.Contains(' '))
            throw new EmailWhiteSpaceException();

        if (!email.Contains('@'))
            throw new EmailWhiteSpaceException("Invaliid Email");


        return email.ToLowerInvariant();

    }
}

public sealed class EmailWhiteSpaceException(string message = "Email cannot contain whitespace") : CustomAppExceptions(message);