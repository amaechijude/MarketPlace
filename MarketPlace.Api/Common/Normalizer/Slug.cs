using System.Text.RegularExpressions;
using MarketPlace.Api.Common.ExceptionHandler;

namespace MarketPlace.Api.Common.Normalizer;

public static class Slugger
{
    public static string Slugify(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
            throw new CategoryException("Category name cannot be less than three characters");

        return name.Trim().ToLowerInvariant().Replace(' ', '-');
    }
}

public sealed class CategoryException(string message) : CustomAppExceptions(message);
