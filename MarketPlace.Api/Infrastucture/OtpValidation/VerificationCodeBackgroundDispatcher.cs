using System.Threading.Channels;
using MarketPlace.Api.Infrastucture.Email;

namespace MarketPlace.Api.Infrastucture.OtpValidation;

public sealed class VerificationCodeBackgroundDispatcher(
    ILogger<VerificationCodeBackgroundDispatcher> logger,
    Channel<OtpEmailRequest> emaiChannel,
    IServiceProvider serviceProvider
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var email in emaiChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await DispatchAsync(email, stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Exception occurred while sending mail to {mail}. on {error}",
                    email.UserEmail,
                    e.Message
                );
            }
        }
    }

    private async Task DispatchAsync(OtpEmailRequest request, CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();

        var htmlBody = EmailTemplates.BuildOtpTemplate("Dear", request.PlainOtp);

        await emailSender.SendEmailAsync(
            new EmailMetaData(request.UserEmail, GetSubject(request.Type), htmlBody),
            cancellationToken
        );
    }

    private static string GetSubject(OtpType type) =>
        type switch
        {
            OtpType.Register => "Registration",
            OtpType.ResetPassword => "Password Reset",
            _ => "Hello",
        };
}
