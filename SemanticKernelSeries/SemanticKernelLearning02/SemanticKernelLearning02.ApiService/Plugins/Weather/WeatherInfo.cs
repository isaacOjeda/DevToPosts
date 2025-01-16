namespace SemanticKernelLearning02.ApiService.Plugins.Weather;

public class WeatherInfo
{
    public string City { get; set; }
    public string Country { get; set; }
    public double Temperature { get; set; }
    public double WindSpeed { get; set; }
    public int WeatherCode { get; set; }
}