using StackExchange.Redis;

namespace MarketPlace.Api;

public sealed class StartupCheck(IConnectionMultiplexer redis, ILogger<StartupCheck> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await redis
                .GetDatabase()
                .PingAsync()
                .WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Redis is not reachable at startup — aborting.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
