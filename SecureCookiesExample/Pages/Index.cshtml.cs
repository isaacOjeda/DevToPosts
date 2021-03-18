using Microsoft.AspNetCore.Mvc.RazorPages;
using SecureCookiesExample.Services;

namespace SecureCookiesExample.Pages
{
    public class IndexPageModel : PageModel
    {
        private readonly SecureCookiesService _secureCookiesService;

        public IndexPageModel(SecureCookiesService secureCookiesService)
        {
            _secureCookiesService = secureCookiesService;
        }

        public void OnGet()
        {
            _secureCookiesService.CreateCookie(".ExampleCookie", new CookieExampleModel
            {
                Name = "Isaac Ojeda",
                UserId = 123456
            });
        }

        public class CookieExampleModel
        {
            public int UserId { get; set; }
            public string Name { get; set; }
        }
    }
}