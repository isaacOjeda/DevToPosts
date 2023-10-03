using DecoratorPattern.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DecoratorPattern.Services;

public class CachedWeatherService : IWeatherService
{
    private readonly IWeatherService _weatherService;
    private readonly IMemoryCache _memoryCache;

    public CachedWeatherService([FromKeyedServices("WeatherService")] IWeatherService weatherService,
        IMemoryCache memoryCache)
    {
        _weatherService = weatherService;
        _memoryCache = memoryCache;
    }

    public async Task<WeatherForecast?> GetWeatherForecastAsync(double latitude, double longitude)
    {
        var key = $"lat={latitude}&lon={longitude}";

        if (_memoryCache.TryGetValue(key, out WeatherForecast? weatherForecast))
        {
            return weatherForecast;
        }

        weatherForecast = await _weatherService.GetWeatherForecastAsync(latitude, longitude);

        _memoryCache.Set(key, weatherForecast, TimeSpan.FromMinutes(15));

        return weatherForecast;
    }
}