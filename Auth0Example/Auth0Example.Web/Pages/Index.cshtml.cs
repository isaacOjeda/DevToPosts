using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auth0Example.Web.Pages;


public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly HttpClient _http;

    public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _http = httpClientFactory.CreateClient("Api");
    }


    public string RawApiResponse { get; set; }


    public async Task OnGet()
    {
        var result = await _http.GetAsync("/me");

        result.EnsureSuccessStatusCode();


        RawApiResponse = await result.Content.ReadAsStringAsync();
    }
}
