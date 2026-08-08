using FluentEmail.Core;
using MailKit.Net.Smtp;
using MailKit.Security;
using MarketPlace.Api.Common.Extensions;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace MarketPlace.Api.Infrastucture.Email;

public sealed class EmailSender(
    ILogger<EmailSender> logger,
    IOptions<SmtpSettings> mailKitOptions,
    IFluentEmail fluentEmail
) : IRequestHandler
{
    private readonly SmtpSettings _mailKitOptions = mailKitOptions.Value;

    public async Task SendEmailAsync(EmailMetaData emailMetaData, CancellationToken ct)
    {
        var name = string.IsNullOrWhiteSpace(emailMetaData.ToName)
            ? emailMetaData.ToEmail[..emailMetaData.ToEmail.IndexOf('@')]
            : emailMetaData.ToName;
        try
        {
            await fluentEmail
                .To(emailMetaData.ToEmail, name)
                .Subject(emailMetaData.Subject)
                .Body(emailMetaData.Body)
                .SendAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occured,. reason {message}", ex.Message);
            throw;
        }
    }
}
