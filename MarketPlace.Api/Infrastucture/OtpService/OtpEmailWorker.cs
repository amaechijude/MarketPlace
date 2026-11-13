using MarketPlace.Api.Infrastucture.Email;
using MarketPlace.Api.Infrastucture.Email.Sendly;

namespace MarketPlace.Api.Infrastucture.OtpService;

public sealed class OtpEmailWorker(
    OtpEmailQueue queue,
    ILogger<OtpEmailWorker> logger,
    IServiceProvider serviceProvider
// IHostEnvironment environment
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var email in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                bool success = false;
                if (!success)
                {
                    success = await DispatchAsync(email, stoppingToken);
                }
                else
                {
                    success = LogForDev(email);
                }
                // log error if failed to send mail, but do not throw exception so that the message can be re-enqueued for retry
                if (!success)
                {
                    logger.LogError(
                        "Failed to send mail to {mail}. Re-enqueing for retry",
                        email.UserEmail
                    );
                }
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
        var emailSender = scope.ServiceProvider.GetRequiredService<SendlyEmailSender>();
        var idempotencyKey = Guid.NewGuid().ToString();
        return await emailSender.SendAsync(
            request.ToSendlyEmailRequest("noreply@gitgbrand.com"),
            idempotencyKey,
            cancellationToken
        );
    }

    private bool LogForDev(OtpEmailRequest email)
    {
        logger.LogError("Code {code}", email.PlainOtp);
        return true;
    }
}
