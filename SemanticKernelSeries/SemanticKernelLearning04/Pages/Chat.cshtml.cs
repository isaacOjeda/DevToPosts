using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SemanticKernelLearning04.Pages;

public class ChatModel : PageModel
{
    private readonly ILogger<ChatModel> _logger;

    public ChatModel(ILogger<ChatModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        // Each time the page is accessed, it will be a new thread
        // The JavaScript will handle creating a new conversation
    }
}