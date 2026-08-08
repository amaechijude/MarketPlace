using MarketPlace.Api.Common.ExceptionHandler;
using MarketPlace.Api.Common.Extensions;
using System.Net;

namespace MarketPlace.Api.Common.Normalizer;

public static class RoleNameNormalizer
{
    public static string Normalize(string roleName)
    {
        roleName = roleName.Trim();
        if (roleName.IsNullOrWhiteSpace() || roleName.ContainsWhiteSpace())
            throw new RoleNameException("Role name cannot be empty or contain whitespace.");

        if (roleName.Length < 3)
            throw new RoleNameException("Role name should be at least 3 characters long.");

        return char.ToUpperInvariant(roleName[0]) + roleName[1..].ToLowerInvariant();
    }
}

public class RoleNameException(
    string message,
    HttpStatusCode statuscode = HttpStatusCode.BadRequest
) : CustomAppExceptions(message, statuscode);
