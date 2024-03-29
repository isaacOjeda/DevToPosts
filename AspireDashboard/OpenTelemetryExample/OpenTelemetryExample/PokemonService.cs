using System.Diagnostics;

namespace OpenTelemetryExample;



public class PokemonService(ILogger<PokemonService> logger, HttpClient http)
{
    public const string ActivitySourceName = "DownloadPokemon";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    public async Task<PokemonList> GetPokemonAsync(int pageSize, int currentPage)
    {
        using var activity = ActivitySource.StartActivity("GetPokemon", ActivityKind.Producer);

        var url = $"https://pokeapi.co/api/v2/pokemon?limit={pageSize}&offset={currentPage * pageSize}";
        var list = await http.GetFromJsonAsync<PokemonList>(url);

        return list ?? new PokemonList
        {
            Next = null,
            Previous = null,
            Results = []
        };
    }
}

public class Pokemon
{
    public required string Name { get; init; }
    public required string Url { get; init; }
}

public class PokemonList
{
    public int Count { get; init; }
    public required string? Next { get; init; }
    public required string? Previous { get; init; }
    public required List<Pokemon> Results { get; init; }
}