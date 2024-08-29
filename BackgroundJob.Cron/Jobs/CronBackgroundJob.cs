using Cronos;

namespace BackgroundJob.Cron.Jobs;

public abstract class CronBackgroundJob : BackgroundService
{
    private readonly CronExpression _cronExpression;
    private readonly TimeZoneInfo _timeZone;

    public CronBackgroundJob(string rawCronExpression, TimeZoneInfo timeZone)
    {
        _cronExpression = CronExpression.Parse(rawCronExpression);
        _timeZone = timeZone;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTimeOffset? nextOccurrence = _cronExpression.GetNextOccurrence(DateTimeOffset.UtcNow, _timeZone);
            
            if (!nextOccurrence.HasValue)
                return;

            var delay = nextOccurrence.Value - DateTimeOffset.UtcNow;
            if (delay.TotalMilliseconds > 0)
            {
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Handle cancellation if needed
                    return;
                }
            }

            try
            {
                await DoWork(stoppingToken);
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
            }
        }
    }

    protected abstract Task DoWork(CancellationToken stoppingToken);
}