using SemanticKernelLearning04.Endpoints;
using SemanticKernelLearning04.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddRazorPages(); // Add Razor Pages support
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddSemanticKernel(builder.Configuration);
builder.Services.AddAgents();

var app = builder.Build();

// Initialize database and configure pipeline
await app.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Enable static files for CSS, JS, etc.
app.UseRouting(); // Add routing

// Redirect root to index page
app.MapGet("/", () => Results.Redirect("/Index"));

// Map API endpoints
app.MapAgentEndpoints();

// Map Razor Pages
app.MapRazorPages();

app.Run();