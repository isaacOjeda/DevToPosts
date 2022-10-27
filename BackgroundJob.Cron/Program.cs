using BackgroundJob.Cron.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCronJob<MySchedulerJob>(options => 
{
    options.CronExpression = "* * * * *";
    options.TimeZone = TimeZoneInfo.Local;
});

var app = builder.Build();

app.Run();
