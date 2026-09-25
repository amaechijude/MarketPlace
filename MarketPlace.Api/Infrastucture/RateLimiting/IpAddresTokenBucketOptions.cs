using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Infrastucture.RateLimiting;

public sealed class IpAddresTokenBucketOptions
{
    [Range(4, 20)]
    public int Capacity { get; set; } = 15;

    [Range(1, 3)]
    public int RefillRate { get; set; } = 1;

    [Range(20, 120)]
    public int RefillIntervalSeconds { get; set; } = 60;

    public const string Key = "IpAddresTokenBucketOptions";
}
