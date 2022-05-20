using Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Api.Worker;

public class WatherWorker : BackgroundService
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<WatherWorker> logger;

    public WatherWorker(IServiceProvider serviceProvider, ILogger<WatherWorker> logger)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            using var scope = serviceProvider.CreateScope();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<WeatherHub>>();

            var data = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            await hubContext.Clients.All.SendAsync("WeatherUpdate", data, stoppingToken);

            await Task.Delay(1000);
        }
    }
}