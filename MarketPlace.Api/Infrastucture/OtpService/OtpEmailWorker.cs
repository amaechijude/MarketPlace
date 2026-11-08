using Amazon.S3.Model;
using MarketPlace.Api.Infrastucture.Email;

namespace MarketPlace.Api.Infrastucture.OtpService;

public sealed class OtpEmailWorker(
    OtpEmailQueue queue,
    ILogger<OtpEmailWorker> logger,
    IServiceProvider serviceProvider
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var email in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                logger.LogError("Code {code}", email.PlainOtp);
                _ = await DispatchAsync(email, stoppingToken);
                // throw new Exception(e)
            }
            catch (Exception e)
            {
                logger.LogError(
                    "Exception occurred while sending mail to {mail}. on {error}",
                    email.UserEmail,
                    e.Message
                );
            }
        }
    }

    private async Task<bool> DispatchAsync(
        OtpEmailRequest request,
        CancellationToken cancellationToken
    )
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();

        return await emailSender.SendEmailAsync(
            new EmailMetaData(request.UserEmail, request.Subject, request.HtmlBody),
            cancellationToken
        );
    }
}
