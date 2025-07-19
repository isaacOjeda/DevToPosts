using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SemanticKernelLearning04.Pages;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;

    public void OnGet()
    {

    }
}