using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Infrastucture.MediaStorage.CloudflareR2;

public sealed class R2Options
{
    [Required, MinLength(20)]
    public string AccountId { get; set; } = string.Empty;

    [Required, MinLength(20)]
    public string AccessKeyId { get; set; } = string.Empty;

    [Required, MinLength(20)]
    public string SecretAccessKey { get; set; } = string.Empty;

    [Required, MinLength(5)]
    public string BucketName { get; set; } = string.Empty;

    [Required, MinLength(20), Url]
    public string ServiceUrl { get; set; } = string.Empty;

    [Required, MinLength(10), Url]
    public string PublicBaseUrl { get; set; } = string.Empty;
}
