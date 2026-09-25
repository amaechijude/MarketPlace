using FluentEmail.Core;
using MarketPlace.Api.Common.Extensions;

namespace MarketPlace.Api.Infrastucture.Email;

public sealed class EmailSender(ILogger<EmailSender> logger, IFluentEmail fluentEmail)
    : IScopedRequestHandler
{
    public async Task<bool> SendEmailAsync(EmailMetaData emailMetaData, CancellationToken ct)
    {
        var name = string.IsNullOrWhiteSpace(emailMetaData.ToName)
            ? emailMetaData.ToEmail[..emailMetaData.ToEmail.IndexOf('@')]
            : emailMetaData.ToName;
        try
        {
            await fluentEmail
                .To(emailMetaData.ToEmail, name)
                .Subject(emailMetaData.Subject)
                .Body(emailMetaData.HtmlBody, isHtml: true)
                .SendAsync(ct);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occured,. reason {message}", ex.Message);
            return false;
        }
    }
}
