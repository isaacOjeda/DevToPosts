using System.Text.Json.Serialization;

namespace DecoratorPattern.Interfaces;

public interface IWeatherService
{
    Task<WeatherForecast?> GetWeatherForecastAsync(double latitude, double longitude);
}

public record WeatherForecast(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("generationtime_ms")] double GenerationtimeMs,
    [property: JsonPropertyName("utc_offset_seconds")] int UtcOffsetSeconds,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("timezone_abbreviation")] string TimezoneAbbreviation,
    [property: JsonPropertyName("elevation")] double Elevation,
    [property: JsonPropertyName("hourly_units")] HourlyUnits HourlyUnits,
    [property: JsonPropertyName("hourly")] Hourly Hourly
);

public record Hourly(
    [property: JsonPropertyName("time")] IReadOnlyList<string> Time,
    [property: JsonPropertyName("temperature_2m")] IReadOnlyList<double> Temperature2m
);

public record HourlyUnits(
    [property: JsonPropertyName("time")] string Time,
    [property: JsonPropertyName("temperature_2m")] string Temperature2m
);

