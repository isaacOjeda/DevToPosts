using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PepInvestigationAgent;

public class TavilySearchTool
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public TavilySearchTool(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = !string.IsNullOrWhiteSpace(apiKey)
            ? apiKey
            : throw new ArgumentException("Tavily API key cannot be empty.", nameof(apiKey));
    }

    [Description("Searches the public web for news, government appointments, official gazettes, sanctions, corporate registries, and family ties of a person.")]
    public async Task<string> SearchWebAsync(
        [Description("The targeted query to search on the web (e.g. 'Jane Doe public office senator declarations')")]
        string query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"\n🔍 [Tool Call: search_web] Searching: \"{query}\"...");
            Console.ResetColor();

            var requestBody = new TavilySearchRequest
            {
                ApiKey = _apiKey,
                Query = query,
                SearchDepth = "advanced",
                IncludeAnswer = true,
                MaxResults = 5
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "https://api.tavily.com/search",
                requestBody,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return $"Search error ({response.StatusCode}): {errorBody}";
            }

            var result = await response.Content.ReadFromJsonAsync<TavilySearchResponse>(cancellationToken: cancellationToken);
            if (result == null || (result.Results?.Count == 0 && string.IsNullOrWhiteSpace(result.Answer)))
            {
                return "No relevant public information found for this query.";
            }

            var formattedResults = new System.Text.StringBuilder();

            if (!string.IsNullOrWhiteSpace(result.Answer))
            {
                formattedResults.AppendLine($"Direct Answer: {result.Answer}\n");
            }

            if (result.Results != null)
            {
                formattedResults.AppendLine("Web Sources:");
                foreach (var item in result.Results)
                {
                    formattedResults.AppendLine($"- Title: {item.Title}");
                    formattedResults.AppendLine($"  URL: {item.Url}");
                    formattedResults.AppendLine($"  Content: {item.Content}");
                    formattedResults.AppendLine();
                }
            }

            return formattedResults.ToString();
        }
        catch (Exception ex)
        {
            return $"Failed to execute search: {ex.Message}";
        }
    }

    private sealed class TavilySearchRequest
    {
        [JsonPropertyName("api_key")]
        public required string ApiKey { get; init; }

        [JsonPropertyName("query")]
        public required string Query { get; init; }

        [JsonPropertyName("search_depth")]
        public string SearchDepth { get; init; } = "advanced";

        [JsonPropertyName("include_answer")]
        public bool IncludeAnswer { get; init; } = true;

        [JsonPropertyName("max_results")]
        public int MaxResults { get; init; } = 5;
    }

    private sealed class TavilySearchResponse
    {
        [JsonPropertyName("answer")]
        public string? Answer { get; init; }

        [JsonPropertyName("results")]
        public List<TavilySearchResult>? Results { get; init; }
    }

    private sealed class TavilySearchResult
    {
        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("url")]
        public string? Url { get; init; }

        [JsonPropertyName("content")]
        public string? Content { get; init; }

        [JsonPropertyName("score")]
        public double? Score { get; init; }
    }
}
