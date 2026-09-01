using MarketPlace.Api.Infrastucture.Email;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net.Sockets;

namespace MarketPlace.Api;

public sealed class StartupCheck(
    IConnectionMultiplexer redis,
    ILogger<StartupCheck> logger,
    IOptions<SmtpSettings> smtpOptions,
    IHostEnvironment env
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (env.IsDevelopment() || env.IsProduction())
        {
            await PingRedisAsync(cancellationToken)
                .WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

            await PingEmailServerAsync(cancellationToken)
                .WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task PingEmailServerAsync(CancellationToken cancellationToken)
    {
        try
        {
            SmtpSettings smtp = smtpOptions.Value;
            using var client = new TcpClient();
            await client.ConnectAsync(smtp.Host, smtp.Port, cancellationToken);
        }
        catch (Exception)
        {
            logger.LogCritical("Email server is not reachable at startup — aborting.");
            throw;
        }
    }

    private async Task PingRedisAsync(CancellationToken cancellationToken)
    {
        try
        {
            var db = redis.GetDatabase();
            var pong = await db.PingAsync();
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Redis ping successful. Latency: {latency} ms",
                    pong.TotalMilliseconds
                );
            }
        }
        catch (Exception)
        {
            logger.LogCritical("Redis is not reachable at startup — aborting.");
            throw;
        }
    }
}
