using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Features.Auth.Login;

public sealed class GoogleSettings
{
    [Required, MinLength(15)]
    public string ClientId { get; set; } = string.Empty;
}
