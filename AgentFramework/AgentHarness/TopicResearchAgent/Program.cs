using System;
using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using TopicResearchAgent;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=========================================================");
Console.WriteLine("  Autonomous Deep Research Agent - Microsoft Agent Harness");
Console.WriteLine("=========================================================\n");

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .Build();

var tavilyApiKey = configuration["Tavily:ApiKey"];
if (string.IsNullOrWhiteSpace(tavilyApiKey))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("[Config] 'Tavily:ApiKey' not configured in user-secrets.");
    Console.ResetColor();
    Console.Write("Enter your Tavily API Key: ");
    tavilyApiKey = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(tavilyApiKey))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: Tavily API Key is required to perform research.");
        Console.ResetColor();
        return;
    }
}

var provider = configuration["Provider"] ?? "OpenRouter";
string endpoint;
string modelId;
string llmApiKey;

if (provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
{
    endpoint = configuration["OpenRouter:Endpoint"] ?? "https://openrouter.ai/api/v1";
    modelId = configuration["OpenRouter:ModelId"] ?? "meta-llama/llama-3.3-70b-instruct";
    llmApiKey = configuration["OpenRouter:ApiKey"] ?? string.Empty;

    if (string.IsNullOrWhiteSpace(llmApiKey))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[Config] 'OpenRouter:ApiKey' not found in user-secrets.");
        Console.ResetColor();
        Console.Write("Enter your OpenRouter API Key: ");
        llmApiKey = Console.ReadLine()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(llmApiKey))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: OpenRouter API Key is required when Provider=OpenRouter.");
            Console.ResetColor();
            return;
        }
    }
}
else
{
    endpoint = configuration["LmStudio:Endpoint"] ?? "http://localhost:1234/v1";
    modelId = configuration["LmStudio:ModelId"] ?? "google/gemma-4-e4b";
    llmApiKey = "lm-studio";
}

var maxIterations = int.TryParse(configuration["Agent:MaxIterations"], out var iter) ? iter : 6;

Console.WriteLine($"[Config] Active Provider: {provider}");
Console.WriteLine($"[Config] LLM Endpoint   : {endpoint}");
Console.WriteLine($"[Config] Model ID       : {modelId}");
Console.WriteLine($"[Config] Max Iterations : {maxIterations}");
Console.WriteLine($"[Config] Search Engine  : Tavily Search API\n");

using var httpClient = new HttpClient();
var tavilyTool = new TavilySearchTool(httpClient, tavilyApiKey);

var openAiClient = new OpenAIClient(
    new ApiKeyCredential(llmApiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri(endpoint)
    });

IChatClient chatClient = openAiClient.GetChatClient(modelId).AsIChatClient();

var harnessOptions = new HarnessAgentOptions
{
    Name = "DeepResearchAgent",
    HarnessInstructions = """
        You are an elite, autonomous technical researcher and architect.
        Your mission is to explore, analyze, and synthesize in-depth knowledge on any given topic.
        Do NOT generate generic, superficial summaries. Use the search_web tool deliberately to gather
        concrete facts, official documentation, architectural designs, benchmarks, and real-world trade-offs.
        Perform multiple distinct searches from different angles (fundamentals, internals, controversies, ecosystem).
        Only emit the final marker RESEARCH_COMPLETE when your structured deep-dive report is fully compiled.
        """,
    ChatOptions = new ChatOptions
    {
        Instructions = """
            # Autonomous Research Protocol

            ## Your Process:
            1. **Deconstruct**: Break the topic into core questions (what, why, how it works under the hood, alternatives, trade-offs).
            2. **Investigate**: Invoke the search_web tool across multiple iterations to collect high-signal technical sources.
            3. **Synthesize**: Produce a comprehensive, senior-level research report with the following structure:
               - **Executive Summary**: High-level overview and current state of the art.
               - **Foundational Concepts**: Core principles, problem space, and architecture.
               - **Deep Technical Breakdown**: Key mechanisms, components, and implementation details.
               - **Trade-offs & Challenges**: Performance, limitations, complexity, and alternatives.
               - **Practical Applications & Ecosystem**: How industry uses it and tooling around it.
               - **Architectural Verdict**: Senior recommendation or conclusions.
               - **Sources & References**: Markdown links with direct URLs discovered during research.

            When your complete, verified report is ready, conclude your final response with the exact marker:
            RESEARCH_COMPLETE
            """,
        Tools =
        [
            AIFunctionFactory.Create(
                tavilyTool.SearchWebAsync,
                "search_web",
                "Searches the public web for documentation, benchmarks, technical papers, articles, and news on any topic.")
        ]
    },
    LoopEvaluators =
    [
        new CompletionMarkerLoopEvaluator("RESEARCH_COMPLETE")
    ],
    LoopAgentOptions = new LoopAgentOptions
    {
        MaxIterations = maxIterations
    }
};

AIAgent agent = chatClient.AsHarnessAgent(harnessOptions);

Console.Write("Enter the topic, technology, or question you want to investigate: ");
var topic = Console.ReadLine();

if (string.IsNullOrWhiteSpace(topic))
{
    Console.WriteLine("No topic provided. Exiting.");
    return;
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"\n[Harness] Starting autonomous deep research loop for: \"{topic}\"...\n");
Console.ResetColor();

var prompt = $"""
    Conduct an autonomous deep-dive investigation into: "{topic}".
    Formulate precise search queries, verify technical specifics, and compile a comprehensive research report.
    """;

try
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("[Harness Live Stream]:");
    Console.ResetColor();

    await foreach (var update in agent.RunStreamingAsync(prompt))
    {
        Console.Write(update.Text);
    }
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[Error] Research process failed: {ex.Message}");
    Console.ResetColor();
}

public partial class Program { }
