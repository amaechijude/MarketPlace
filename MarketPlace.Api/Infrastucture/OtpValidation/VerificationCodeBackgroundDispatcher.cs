using MarketPlace.Api.Infrastucture.Email;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace MarketPlace.Api.Infrastucture.OtpValidation;

public sealed class VerificationCodeBackgroundDispatcher(
    ILogger<VerificationCodeBackgroundDispatcher> logger,
    Channel<OtpEmailRequest> emaiChannel,
    IServiceProvider serviceProvider
) : BackgroundService
{
    public static readonly ConcurrentQueue<OtpEmailRequest> RequestQueue = [];

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

        var isSuccessful = await emailSender.SendEmailAsync(
            new EmailMetaData(request.UserEmail, request.Subject, request.HtmlBody),
            cancellationToken
        );
        if (!isSuccessful)
            RequestQueue.Enqueue(request);
    }
}


public sealed class VerificationCodeBackgroundDispatcher1(
    ILogger<VerificationCodeBackgroundDispatcher1> logger,
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

        var isSuccessful = await emailSender.SendEmailAsync(
            new EmailMetaData(request.UserEmail, request.Subject, request.HtmlBody),
            cancellationToken
        );
        if (!isSuccessful)
            VerificationCodeBackgroundDispatcher.RequestQueue.Enqueue(request);
    }
}


public sealed class VerificationCodeBackgroundDispatcher4(
    ILogger<VerificationCodeBackgroundDispatcher4> logger,
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

        var isSuccessful = await emailSender.SendEmailAsync(
            new EmailMetaData(request.UserEmail, request.Subject, request.HtmlBody),
            cancellationToken
        );
        if (!isSuccessful)
            VerificationCodeBackgroundDispatcher.RequestQueue.Enqueue(request);
    }
}



public sealed class VerificationCodeBackgroundDispatcher2(
    ILogger<VerificationCodeBackgroundDispatcher2> logger,
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

        var isSuccessful = await emailSender.SendEmailAsync(
            new EmailMetaData(request.UserEmail, request.Subject, request.HtmlBody),
            cancellationToken
        );
        if (!isSuccessful)
            VerificationCodeBackgroundDispatcher.RequestQueue.Enqueue(request);
    }
}



public sealed class VerificationCodeBackgroundDispatcher3(
    ILogger<VerificationCodeBackgroundDispatcher3> logger,
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

        var isSuccessful = await emailSender.SendEmailAsync(
            new EmailMetaData(request.UserEmail, request.Subject, request.HtmlBody),
            cancellationToken
        );
        if (!isSuccessful)
            VerificationCodeBackgroundDispatcher.RequestQueue.Enqueue(request);
    }
}
