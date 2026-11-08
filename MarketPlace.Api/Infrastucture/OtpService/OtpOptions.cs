using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Infrastucture.OtpService;

public sealed class OtpOptions
{
    [Required, MinLength(64)]
    public string Secret { get; set; } = string.Empty;
}
