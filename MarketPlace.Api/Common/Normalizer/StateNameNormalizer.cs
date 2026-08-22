namespace MarketPlace.Api.Common.Normalizer;

public static class StateNameNormalizer
{
    public static string Normalize(string name) => name.Trim().Replace(' ', '-').ToLowerInvariant();
}
