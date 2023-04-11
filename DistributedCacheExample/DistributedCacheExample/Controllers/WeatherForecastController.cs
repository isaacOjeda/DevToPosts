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
    private readonly DistributedCacheWrapper _cacheWrapper;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, DistributedCacheWrapper cacheWrapper)
    {
        _logger = logger;
        _cacheWrapper = cacheWrapper;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        var data = await _cacheWrapper.GetCachedItem<IEnumerable<WeatherForecast>>("GetWeatherForecast");

        if (data is null)
        {
            data = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            await _cacheWrapper.SaveItem(data, "GetWeatherForecast", expirationInMinutes: 5);
        }

        return data;
    }
}
