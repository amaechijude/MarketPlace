namespace MarketPlace.Api.Infrastucture.Cache;

public static class CacheKeys
{
    public static string ShippingFeeById(int id) => $"ShippingFee-Id-{id}";

    public const string ListOfShippingFees = "ShippingFee-List";

    public static string Product(Guid productId) => $"product-{productId}";
}
