using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace RestEasePollyExample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IGitHubApi _github;

        public IndexModel(ILogger<IndexModel> logger, IGitHubApi github)
        {
            _logger = logger;
            _github = github;
        }

        public User GitHubUser { get; set; }

        public async Task OnGet(string userName)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                GitHubUser = await _github.GetUserAsync(userName);
            }
        }
    }
}
