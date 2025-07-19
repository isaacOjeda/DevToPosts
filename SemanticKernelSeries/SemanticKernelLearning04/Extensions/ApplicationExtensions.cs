using SemanticKernelLearning04.Data;
using SemanticKernelLearning04.Services;

namespace SemanticKernelLearning04.Extensions;

public static class ApplicationExtensions
{
    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConversationDbContext>();
        var invoiceService = scope.ServiceProvider.GetRequiredService<InvoiceService>();

        await context.Database.EnsureCreatedAsync();
        await invoiceService.SeedSampleDataAsync();

        return app;
    }
}