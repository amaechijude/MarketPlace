namespace MarketPlace.Api.Common.Extensions
{
    public static class GuidExtension
    {
        public static bool IsEmpty(this Guid guid) => guid == Guid.Empty;
    }
}
