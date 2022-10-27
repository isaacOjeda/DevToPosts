namespace BackgroundJob.Cron.Jobs;

public static class CronBackgroundJobExtensions
{
    public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<CronSettings<T>> options)
        where T: CronBackgroundJob
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var config = new CronSettings<T>();
        options.Invoke(config);

        if (string.IsNullOrWhiteSpace(config.CronExpression))
        {
            throw new ArgumentNullException(nameof(CronSettings<T>.CronExpression));
        }

        services.AddSingleton<CronSettings<T>>(config);
        services.AddHostedService<T>();

        return services;
    }
}

