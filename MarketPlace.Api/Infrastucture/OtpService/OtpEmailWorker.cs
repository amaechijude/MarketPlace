using MarketPlace.Api.Infrastucture.Email;

namespace MarketPlace.Api.Infrastucture.OtpService;

public sealed class OtpEmailWorker(
    OtpEmailQueue queue,
    ILogger<OtpEmailWorker> logger,
    IServiceProvider serviceProvider,
    IHostEnvironment environment
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var email in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                bool success = false;
                if (environment.IsProduction())
                {
                    success = await DispatchAsync(email, stoppingToken);
                }
                else
                {
                    success = LogForDev(email);
                }
                ;
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
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();

            return await emailSender.SendEmailAsync(
                new EmailMetaData(request.UserEmail, request.Subject, request.HtmlBody),
                cancellationToken
            );
        }
    }

    private bool LogForDev(OtpEmailRequest email)
    {
        logger.LogError("Code {code}", email.PlainOtp);
        return true;
    }
}
