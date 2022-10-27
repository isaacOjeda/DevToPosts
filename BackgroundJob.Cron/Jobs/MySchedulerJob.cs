namespace BackgroundJob.Cron.Jobs;

public class MySchedulerJob : CronBackgroundJob
{
    private readonly ILogger<MySchedulerJob> _log;

    public MySchedulerJob(CronSettings<MySchedulerJob> settings, ILogger<MySchedulerJob> log)
        :base(settings.CronExpression, settings.TimeZone)
    {
        _log = log;
    }
    
    protected override Task DoWork(CancellationToken stoppingToken)
    {
        _log.LogInformation("Running... at {0}", DateTime.UtcNow);

        return Task.CompletedTask;
    }
}