using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace WebClient.Pages
{
    public class MeModel : PageModel
    {
        private readonly HttpClient _http;

        public MeModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
        }

        public string RawJson { get; set; } = default!;

        public async Task OnGet()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _http.GetAsync("https://localhost:7005/me");

            response.EnsureSuccessStatusCode();

            RawJson = await response.Content.ReadAsStringAsync();
        }
    }
}
