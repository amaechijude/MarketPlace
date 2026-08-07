using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Infrastucture.Email;

public sealed class MailKitSettings
{
    [MinLength(7)]
    public string SmtpHost { get; set; } = string.Empty;

    [MinLength(2)]
    public string SmtpPort { get; set; } = string.Empty;

    [MinLength(5)]
    public string Username { get; set; } = string.Empty;

    [MinLength(7)]
    public string Password { get; set; } = string.Empty;

    [MinLength(5), EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    [MinLength(7)]
    public string FromName { get; set; } = string.Empty;

    // public bool UseSsl { get; set; }

    public int TimeoutInSeconds { get; set; } = 30;
}
