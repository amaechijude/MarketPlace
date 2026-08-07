using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace MarketPlace.Api.Infrastucture.Email;

public sealed class EmailSender(
    IHostEnvironment environment,
    ILogger<EmailSender> logger,
    IOptions<MailKitSettings> mailKitOptions
)
{
    private readonly MailKitSettings _mailKitOptions = mailKitOptions.Value;

    public async Task SendEmailAsync(
        IEnumerable<string> toEmails,
        string toName,
        string subject,
        string body,
        CancellationToken ct
    )
    {
        var enumerable = toEmails as string[] ?? [.. toEmails];
        if (enumerable.Length == 0)
            return;

        var mimeMessage = new MimeMessage();

        mimeMessage.From.Add(
            new MailboxAddress(_mailKitOptions.FromName, _mailKitOptions.FromEmail)
        );
        mimeMessage.To.AddRange(enumerable.Select(email => new MailboxAddress(toName, email)));

        mimeMessage.Subject = subject;

        mimeMessage.Body = new TextPart("html") { Text = body };

        if (environment.IsProduction())
        {
            await SendViaMailKitAsync(mimeMessage, ct);
        }
        else
        {
            await SendToPapercut(mimeMessage, ct);
        }
    }

    private async Task SendToPapercut(MimeMessage mimeMessage, CancellationToken ct)
    {
        using var client = new SmtpClient();
        try
        {
            var secureSocketOptions = environment.IsProduction()
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            client.Timeout = _mailKitOptions.TimeoutInSeconds * 1000; // milliseconds

            await client.ConnectAsync(
                host: _mailKitOptions.SmtpHost,
                port: Convert.ToInt32(_mailKitOptions.SmtpPort),
                options: secureSocketOptions,
                cancellationToken: ct
            );

            // await client.AuthenticateAsync(
            //     userName: _mailKitOptions.Username,
            //     password: _mailKitOptions.Password,
            //     cancellationToken: ct
            // );

            await client.SendAsync(message: mimeMessage, cancellationToken: ct);
        }
        finally
        {
            await client.DisconnectAsync(quit: true, cancellationToken: ct);
        }
    }

    private async Task SendViaMailKitAsync(MimeMessage mimeMessage, CancellationToken ct)
    {
        try
        {
            await _resiliencePipeline.ExecuteAsync(
                async cancellationToken =>
                {
                    using var client = new SmtpClient();

                    try
                    {
                        var secureSocketOptions = environment.IsProduction()
                            ? SecureSocketOptions.StartTls
                            : SecureSocketOptions.None;

                        client.Timeout = _mailKitOptions.TimeoutInSeconds * 1000; // milliseconds

                        await client.ConnectAsync(
                            host: _mailKitOptions.SmtpHost,
                            port: Convert.ToInt32(_mailKitOptions.SmtpPort),
                            options: secureSocketOptions,
                            cancellationToken: cancellationToken
                        );

                        await client.AuthenticateAsync(
                            userName: _mailKitOptions.Username,
                            password: _mailKitOptions.Password,
                            cancellationToken: cancellationToken
                        );

                        await client.SendAsync(
                            message: mimeMessage,
                            cancellationToken: cancellationToken
                        );
                    }
                    finally
                    {
                        await client.DisconnectAsync(
                            quit: true,
                            cancellationToken: cancellationToken
                        );
                    }
                },
                ct
            );
        }
        catch (BrokenCircuitException ex)
        {
            logger.LogError(ex, "[Mail] Send aborted — circuit is open, SMTP has been unavailable");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "[Mail] Failed to send email after all retry attempts");
        }
    }

    private readonly ResiliencePipeline _resiliencePipeline = new ResiliencePipelineBuilder()
        .AddTimeout(TimeSpan.FromSeconds(30))
        .AddRetry(
            new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<SmtpCommandException>(ex => (int)ex.StatusCode >= 500)
                    .Handle<SmtpProtocolException>()
                    .Handle<System.Net.Sockets.SocketException>()
                    .Handle<System.IO.IOException>()
                    .Handle<System.TimeoutException>(),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromSeconds(2),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        args.Outcome.Exception,
                        "[Mail] Retry attempt {RetryAttempt} after {Delay}s due to {ExceptionType}: {Message}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalSeconds,
                        args.Outcome.Exception?.GetType().Name,
                        args.Outcome.Exception?.Message
                    );
                    return default;
                },
            }
        )
        .AddCircuitBreaker(
            new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<SmtpCommandException>(ex => (int)ex.StatusCode >= 500)
                    .Handle<SmtpProtocolException>()
                    .Handle<System.Net.Sockets.SocketException>()
                    .Handle<System.IO.IOException>()
                    .Handle<System.TimeoutException>(),
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromMinutes(2),
                MinimumThroughput = 8,
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    logger.LogError(
                        args.Outcome.Exception,
                        "[Mail] Circuit breaker opened for {BreakDuration}s due to: {Message}",
                        args.BreakDuration.TotalSeconds,
                        args.Outcome.Exception?.Message
                    );
                    return default;
                },
                OnClosed = _ =>
                {
                    logger.LogInformation(
                        "[Mail] Circuit breaker closed. Resuming normal operations."
                    );
                    return default;
                },
            }
        )
        .Build();
}
