using MarketPlace.Api.Infrastucture.Email;

namespace MarketPlace.Api.Infrastucture.OtpValidation;

public sealed class VerificationCodeBackgroundDead(IServiceProvider serviceProvider)
    : BackgroundService
{
    private static readonly TimeSpan timeSpan = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(timeSpan);
        while (
            stoppingToken is { IsCancellationRequested: false }
            && await timer.WaitForNextTickAsync(stoppingToken)
        )
        {
            if (VerificationCodeBackgroundDispatcher.RequestQueue.TryDequeue(out var request))
                await DispatchAsync(request, stoppingToken);
        }
    }

    private async Task DispatchAsync(OtpEmailRequest request, CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();

        var isSuccessful = await emailSender.SendEmailAsync(
            new EmailMetaData(request.UserEmail, request.Subject, request.HtmlBody),
            cancellationToken
        );
        if (!isSuccessful)
            VerificationCodeBackgroundDispatcher.RequestQueue.Enqueue(request);
    }
}
