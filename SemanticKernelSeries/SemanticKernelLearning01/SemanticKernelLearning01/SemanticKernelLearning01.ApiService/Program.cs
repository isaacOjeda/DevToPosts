using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;
using System.Data.Common;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var kernel = builder.Services.AddKernel();

var (endpoint, modelId) = GetOllamaConnectionString();

#pragma warning disable SKEXP0070
kernel.AddOllamaTextGeneration(modelId, endpoint);
kernel.AddOllamaChatCompletion(modelId, endpoint);
#pragma warning restore SKEXP0070

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.MapPost("/api/summarizer", async (
    TextCompletionRequest request,
    ITextGenerationService textGenerationService) =>
{
    var prompt = $"""
                 Summarize the following text in one sentence:  

                 {request.Text}
                 """;

    var response = await textGenerationService.GetTextContentsAsync(prompt);

    return response;
});

app.MapPost("/api/v2/summarizer", async (TextCompletionRequest request, Kernel kernel) =>
{
    var prompt = """
                 Summarize the following text in one sentence:  
                 
                 {{$text}}
                 """;

    var arguments = new KernelArguments()
    {
        ["text"] = request.Text
    };

    var response = await kernel.InvokePromptAsync(prompt, arguments);

    return Results.Ok(new
    {
        Response = response.ToString()
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

public record TextCompletionRequest(string Text);