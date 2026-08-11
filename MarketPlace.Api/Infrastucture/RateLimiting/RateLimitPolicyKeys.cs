namespace MarketPlace.Api.Infrastucture.RateLimiting;

public static class RateLimitPolicyKeys
{
    public const string LoginTokenBucket = "loginTokenBucket";
    public const string NewsLetterFixed = "NewsLetterFixed";
    public const string AddressTokenBucket = "AddressTokenBucket";
    public const string RegisterTokenBucket = "RegisterTokenBucket";
}
