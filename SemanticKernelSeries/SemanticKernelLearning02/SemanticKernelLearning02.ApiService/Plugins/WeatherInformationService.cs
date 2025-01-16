using System.ComponentModel;
using Microsoft.SemanticKernel;
using SemanticKernelLearning02.ApiService.Plugins.Weather;

namespace SemanticKernelLearning02.ApiService.Plugins;

public class WeatherInformationService(IHttpClientFactory httpClientFactory)
{
    [KernelFunction]
    [Description("Get information for a city.")]
    public async Task<GeographicInfo> GetCityInformation(string cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            throw new ArgumentException("City name cannot be null or empty.", nameof(cityName));

        var geoInfo = await GetCityCoordinatesAsync(cityName);

        return new GeographicInfo
        {
            Country = geoInfo.Country,
            Latitude = geoInfo.Latitude,
            Longitude = geoInfo.Longitude,
            Name = geoInfo.Name,
            Elevation = geoInfo.Elevation,
            Timezone = geoInfo.Timezone,
            Population = geoInfo.Population
        };
    }


    [KernelFunction]
    [Description("Get weather information for a city.")]
    public async Task<WeatherInfo> GetWeatherByCity(string cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            throw new ArgumentException("City name cannot be null or empty.", nameof(cityName));

        // Step 1: Get city coordinates
        var city = await GetCityCoordinatesAsync(cityName);

        // Step 2: Get weather data
        var weatherResult = await GetWeatherDataAsync(city.Latitude, city.Longitude);

        return new WeatherInfo
        {
            City = city.Name,
            Country = city.Country,
            Temperature = weatherResult.CurrentWeather.Temperature,
            WindSpeed = weatherResult.CurrentWeather.Windspeed,
            WeatherCode = weatherResult.CurrentWeather.Weathercode
        };
    }

    private async Task<GeocodingResult> GetCityCoordinatesAsync(string cityName)
    {
        var geoCodingClient = httpClientFactory.CreateClient("GeoCodingOpenMeteo");
        var geocodingUrl = $"/v1/search?name={Uri.EscapeDataString(cityName)}&count=1&language=es&format=json";
        var geocodingResponse = await geoCodingClient.GetAsync(geocodingUrl);
        geocodingResponse.EnsureSuccessStatusCode();

        var geocodingResult = await geocodingResponse.Content.ReadFromJsonAsync<GeocodingResponse>();

        if (geocodingResult?.Results == null || geocodingResult.Results.Count == 0)
            throw new Exception("City not found.");

        return geocodingResult.Results[0];
    }

    private async Task<WeatherApiResponse> GetWeatherDataAsync(double latitude, double longitude)
    {
        var weatherClient = httpClientFactory.CreateClient("OpenMeteo");
        var weatherUrl = $"/v1/forecast?latitude={latitude}&longitude={longitude}&current_weather=true";
        var weatherResponse = await weatherClient.GetAsync(weatherUrl);
        weatherResponse.EnsureSuccessStatusCode();

        var weatherResult = await weatherResponse.Content.ReadFromJsonAsync<WeatherApiResponse>();

        return weatherResult;
    }
}