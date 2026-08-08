namespace MarketPlace.Api.Common.Extensions;

public static class StringExtensions
{
    public static bool IsAllAsciiDigits(this string value) => value.All(char.IsAsciiDigit);

    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    public static bool ContainsWhiteSpace(this string value) => value.Trim().Contains(' ');
}
