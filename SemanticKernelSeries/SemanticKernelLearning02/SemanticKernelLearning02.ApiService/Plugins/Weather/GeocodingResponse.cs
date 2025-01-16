using System.Text.Json.Serialization;

namespace SemanticKernelLearning02.ApiService.Plugins.Weather;

public class GeocodingResponse
{
    public List<GeocodingResult> Results { get; set; }
}

public class GeocodingResult
{
    public int Id { get; set; }

    public string Name { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double Elevation { get; set; }

    [JsonPropertyName("feature_code")] public string FeatureCode { get; set; }

    [JsonPropertyName("country_code")] public string CountryCode { get; set; }

    public string Timezone { get; set; }

    public long Population { get; set; }

    [JsonPropertyName("country_id")] public int CountryId { get; set; }

    public string Country { get; set; }

    public string Admin1 { get; set; }

    public string Admin2 { get; set; }
}