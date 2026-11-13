using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Infrastucture.Email.Sendly;

public sealed class SendlyEmailOptions
{
    [Required, MinLength(5)]
    public string ApiKey { get; set; } = string.Empty;

    [Required, Url]
    public string Baseurl { get; set; } = "https://api.sendlyai.com";
}
