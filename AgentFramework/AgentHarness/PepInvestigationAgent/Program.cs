using System;
using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using PepInvestigationAgent;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=================================================");
Console.WriteLine("  PEP Investigation Agent - Microsoft Agent Harness");
Console.WriteLine("=================================================\n");

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
        Console.WriteLine("Error: Tavily API Key is required to run web investigations.");
        Console.ResetColor();
        return;
    }
}

var provider = configuration["Provider"] ?? "LmStudio";
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
Console.WriteLine($"[Config] Search Engine   : Tavily Search API\n");

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
    Name = "PepInvestigator",
    HarnessInstructions = """
        You are an autonomous intelligence investigator specializing in Politically Exposed Persons (PEP) compliance.
        Your mission is to perform deep investigative research using the search_web tool to gather concrete public evidence.
        Do not stop after a single generic query; investigate roles, past government appointments, corporate boards, and family ties.
        Only emit the final marker INVESTIGATION_COMPLETE when your final structured report is fully compiled.
        """,
    ChatOptions = new ChatOptions
    {
        Instructions = """
            # Investigation Guidelines for Politically Exposed Persons (PEP)

            A Politically Exposed Person (PEP) is an individual entrusted with a prominent public function:
            - Senior government, military, judicial, or political party officials.
            - Executives of state-owned enterprises.
            - Immediate family members (spouses, children, parents, siblings) and close associates of PEPs.

            ## Your Process:
            1. Formulate search queries to find public office appointments, government gazettes, and official roles.
            2. Search for family connections, political alliances, and company ownership.
            3. Synthesize the findings into a structured report with:
               - **Subject Profile**: Full name, known roles, country.
               - **Public Office History**: Roles held, dates, government branches.
               - **Family & Business Connections**: Relevant ties to other public figures.
               - **PEP Classification**: [Direct PEP | PEP by Association | Not a PEP | Inconclusive].
               - **Confidence & Risk Level**: [High | Medium | Low].
               - **Sources**: Direct URLs discovered during search.

            When the report is complete and verified with sources, end your final answer with the exact marker:
            INVESTIGATION_COMPLETE
            """,
        Tools =
        [
            AIFunctionFactory.Create(
                tavilyTool.SearchWebAsync,
                "search_web",
                "Searches the public web for appointments, news, government gazettes, corporate registries, and family ties.")
        ]
    },
    LoopEvaluators =
    [
        new CompletionMarkerLoopEvaluator("INVESTIGATION_COMPLETE")
    ],
    LoopAgentOptions = new LoopAgentOptions
    {
        MaxIterations = maxIterations
    }
};

AIAgent agent = chatClient.AsHarnessAgent(harnessOptions);

Console.Write("Enter the full name of the person to investigate (and country if known): ");
var targetPerson = Console.ReadLine();

if (string.IsNullOrWhiteSpace(targetPerson))
{
    Console.WriteLine("No target provided. Exiting.");
    return;
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"\n[Harness] Starting autonomous investigation loop for: \"{targetPerson}\"...\n");
Console.ResetColor();

var prompt = $"Investigate whether '{targetPerson}' is a Politically Exposed Person (PEP) or has direct ties to PEPs. Conduct thorough web searches and produce the structured compliance report.";

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
    Console.WriteLine($"\n[Error] Investigation failed: {ex.Message}");
    Console.ResetColor();
}

public partial class Program { }
