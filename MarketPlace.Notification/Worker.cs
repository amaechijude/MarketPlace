namespace MarketPlace.Notification;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    private static readonly TimeSpan tick = TimeSpan.FromSeconds(2);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(tick);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        }
    }
}