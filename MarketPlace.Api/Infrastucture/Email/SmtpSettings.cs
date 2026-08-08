using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Infrastucture.Email;

public sealed class SmtpSettings
{
    [MinLength(7)]
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    [MinLength(5)]
    public string Username { get; set; } = string.Empty;

    [MinLength(7)]
    public string Password { get; set; } = string.Empty;

    [MinLength(5), EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    [MinLength(7)]
    public string FromName { get; set; } = string.Empty;

    public int TimeoutInSeconds { get; set; } = 30;
}
