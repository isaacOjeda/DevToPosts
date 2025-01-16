using System.Text.Json.Serialization;

namespace SemanticKernelLearning02.ApiService.Plugins.Weather;

public class WeatherApiResponse
{
    [JsonPropertyName("latitude")] public double Latitude { get; set; }

    [JsonPropertyName("longitude")] public double Longitude { get; set; }

    [JsonPropertyName("generationtime_ms")]
    public double GenerationtimeMs { get; set; }

    [JsonPropertyName("utc_offset_seconds")]
    public int UtcOffsetSeconds { get; set; }

    [JsonPropertyName("timezone")] public string Timezone { get; set; }

    [JsonPropertyName("timezone_abbreviation")]
    public string TimezoneAbbreviation { get; set; }

    [JsonPropertyName("elevation")] public double Elevation { get; set; }

    [JsonPropertyName("current_weather_units")]
    public CurrentWeatherUnits CurrentWeatherUnits { get; set; }

    [JsonPropertyName("current_weather")] public CurrentWeather CurrentWeather { get; set; }
}


public class CurrentWeatherUnits
{
    [JsonPropertyName("time")] public string Time { get; set; }

    [JsonPropertyName("interval")] public string Interval { get; set; }

    [JsonPropertyName("temperature")] public string Temperature { get; set; }

    [JsonPropertyName("windspeed")] public string Windspeed { get; set; }

    [JsonPropertyName("winddirection")] public string Winddirection { get; set; }

    [JsonPropertyName("is_day")] public string IsDay { get; set; }

    [JsonPropertyName("weathercode")] public string Weathercode { get; set; }
}

public class CurrentWeather
{
    [JsonPropertyName("time")] public string Time { get; set; }

    [JsonPropertyName("interval")] public int Interval { get; set; }

    [JsonPropertyName("temperature")] public double Temperature { get; set; }

    [JsonPropertyName("windspeed")] public double Windspeed { get; set; }

    [JsonPropertyName("winddirection")] public int Winddirection { get; set; }

    [JsonPropertyName("is_day")] public int IsDay { get; set; }

    [JsonPropertyName("weathercode")] public int Weathercode { get; set; }
}