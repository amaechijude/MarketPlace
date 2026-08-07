using MarketPlace.Api.Common.ExceptionHandler;
using MarketPlace.Api.Common.Extensions;

namespace MarketPlace.Api.Common.Normalizer;

public static class EmailNormalizer
{
    public static string Normalize(string email)
    {
        email = email.Trim();
        if (email.ContainsWhiteSpace())
        {
            throw new EmailWhiteSpaceException();
        }

        return email.ToLowerInvariant();

    }
}

public sealed class EmailWhiteSpaceException() : CustomAppExceptions("Email cannot contain whitespace");