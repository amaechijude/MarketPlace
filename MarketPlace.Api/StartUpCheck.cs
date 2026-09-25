using StackExchange.Redis;

namespace MarketPlace.Api;

public sealed class StartupCheck(IConnectionMultiplexer redis, ILogger<StartupCheck> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IEnumerable<Task> tasks = [PingRedisAsync(cancellationToken)];

        await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

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
