namespace MarketPlace.Api.Common.Extensions;

public static class EnumerableExtension
{
    extension<T>(IEnumerable<T> values)
    {
        public bool DoesNotContain(T data) => !values.Contains(data);
    }
}
