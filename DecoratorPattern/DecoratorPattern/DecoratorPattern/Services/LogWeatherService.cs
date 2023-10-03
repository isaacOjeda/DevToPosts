using DecoratorPattern.Interfaces;

namespace DecoratorPattern.Services;

public class LogWeatherService : IWeatherService
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<LogWeatherService> _logger;

    public LogWeatherService([FromKeyedServices("CachedWeatherService")] IWeatherService weatherService,
        ILogger<LogWeatherService> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    public async Task<WeatherForecast?> GetWeatherForecastAsync(double latitude, double longitude)
    {
        _logger.LogInformation("Getting weather forecast for {Latitude}, {Longitude}", latitude, longitude);

        var response = await _weatherService.GetWeatherForecastAsync(latitude, longitude);

        _logger.LogInformation("Got weather forecast for {Latitude}, {Longitude}", latitude, longitude);

        return response;
    }
}