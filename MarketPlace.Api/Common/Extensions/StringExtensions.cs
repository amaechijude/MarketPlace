namespace MarketPlace.Api.Common.Extensions;

public static class StringExtensions
{
    public static bool IsAllAsciiDigits(this string value) => value.All(char.IsAsciiDigit);

    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    public static bool ContainsWhiteSpace(this string value) => value.Trim().Contains(' ');

    public static bool HasNoWhiteSpaceBetweenChar(this string value) => !value.ContainsWhiteSpace();

    public static bool ContainsSpecialCharacter(this string value) =>
        value.Any(ch => !char.IsAsciiLetterOrDigit(ch) && !char.IsWhiteSpace(ch));

    public static bool CharCountIsGreaterThanOrEqual(this string value, char ch, int count) =>
        value.Count(c => c == ch) >= count;

    public static bool CharCountIsEqual(this string value, char ch, int count) =>
        value.Count(c => c == ch) == count;
}
