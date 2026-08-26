namespace MarketPlace.Api.Common.Extensions;

public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    extension(string value)
    {
        public bool ContainsWhiteSpace() => value.Trim().Contains(' ');
        public bool HasNoWhiteSpaceBetweenChar() => !value.ContainsWhiteSpace();

        public bool ContainsSpecialCharacter() =>
            value.Any(ch => !char.IsAsciiLetterOrDigit(ch) && !char.IsWhiteSpace(ch));

        public bool CharCountIsGreaterThanOrEqual(char ch, int count) =>
            value.Count(c => c == ch) >= count;

        public bool IsAllAsciiDigits() => value.All(char.IsAsciiDigit);
    }
}
