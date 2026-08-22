namespace MarketPlace.Api.Common.Extensions
{
    public static class GuidExtension
    {
        extension(Guid guid)
        {
            public bool IsEmpty => guid == Guid.Empty;
            public bool IsNotEmpty => guid != Guid.Empty;
        }
    }
}
