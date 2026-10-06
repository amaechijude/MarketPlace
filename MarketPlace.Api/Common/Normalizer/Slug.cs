using MarketPlace.Api.Common.ExceptionHandler;

namespace MarketPlace.Api.Common.Normalizer;

public static class Slugger
{
    public static string Slugify(string name) => name.Trim().ToLowerInvariant().Replace(' ', '-');
}
