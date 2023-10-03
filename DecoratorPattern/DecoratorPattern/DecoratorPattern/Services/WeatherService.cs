using DecoratorPattern.Interfaces;

namespace DecoratorPattern.Services;

public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WeatherService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<WeatherForecast?> GetWeatherForecastAsync(double latitude, double longitude)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri("https://api.open-meteo.com");

        var response =
            await httpClient.GetFromJsonAsync<WeatherForecast>(
                $"v1/forecast?latitude={latitude}&longitude={longitude}&hourly=temperature_2m");

        if (response is null)
        {
            throw new Exception("Unable to retrieve weather forecast.");
        }

        return response;
    }
}