#pragma warning disable SKEXP0070

using System.Data.Common;
using System.Net;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelLearning02.ApiService.Plugins;

var builder = WebApplication.CreateBuilder(args);


builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient("OpenMeteo", http =>
{
    http.BaseAddress = new Uri("https://api.open-meteo.com");
});
builder.Services.AddHttpClient("GeoCodingOpenMeteo", http =>
{
    http.BaseAddress = new Uri("https://geocoding-api.open-meteo.com");
});

var kernelBuilder = builder.Services.AddKernel();
var (endpoint, modelId) = GetOllamaConnectionString();

kernelBuilder.AddOllamaChatCompletion(modelId, endpoint);
kernelBuilder.Plugins.AddFromType<TimeInformationService>();
kernelBuilder.Plugins.AddFromType<WeatherInformationService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.MapPost("/api/chat", async (ChatRequest request, Kernel kernel) =>
{
    var settings = new OpenAIPromptExecutionSettings()
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };

    var response = await kernel.InvokePromptAsync(request.Question, new(settings));

    return Results.Ok(new
    {
        Result = response.ToString()
    });
});

app.Run();


(Uri endpoint, string modelId) GetOllamaConnectionString()
{
    var connectionString = builder.Configuration.GetConnectionString("llama");

    var connectionBuilder = new DbConnectionStringBuilder
    {
        ConnectionString = connectionString
    };

    Uri endpoint = new Uri(connectionBuilder["Endpoint"].ToString());
    string modelId = connectionBuilder["Model"].ToString();

    return (endpoint, modelId);
}

public record ChatRequest(string Question);

#pragma warning restore SKEXP0070