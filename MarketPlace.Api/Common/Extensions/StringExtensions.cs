
namespace MarketPlace.Api.Common.Extensions;

public static class StringExtensions
{
    public static bool IsAllAsciiDigits(this string? value) => !string.IsNullOrWhiteSpace(value) && value.All(char.IsAsciiDigit);
    public static bool ContainsWhiteSpace(this string? value) => !string.IsNullOrWhiteSpace(value) && value.Trim().Contains(' ');
}
