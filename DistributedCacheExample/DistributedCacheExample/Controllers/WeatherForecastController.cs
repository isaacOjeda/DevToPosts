using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace DistributedCacheExample.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IDistributedCache _distributedCache;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedCache distributedCache)
    {
        _logger = logger;
        _distributedCache = distributedCache;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        var dataInBytes = await _distributedCache.GetAsync("GetWeatherForecast");
        var data = Enumerable.Empty<WeatherForecast>();

        if (dataInBytes is null)
        {
            data = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            var dataJson = JsonSerializer.Serialize(data);
            dataInBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);

            _distributedCache.Set("GetWeatherForecast", dataInBytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(5)
            });
        }

        var rawJson = System.Text.Encoding.UTF8.GetString(dataInBytes);
        data = JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(rawJson);

        return data;
    }
}
