using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelLearning04.Services;

namespace SemanticKernelLearning04.Endpoints;

public static class AgentEndpoints
{
    public static WebApplication MapAgentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/agent")
            .WithTags("Agent")
            .WithOpenApi();

        group.MapPost("/start", async (ConversationService conversationService) =>
        {
            // Create a new conversation in the database
            var conversationId = await conversationService.CreateConversationAsync();
            return Results.Ok(new { ConversationId = conversationId });
        });

        group.MapPost("/", async (AskQuestionRequest request,
            [FromKeyedServices("AssistantAgent")] ChatCompletionAgent agent,
            ConversationService conversationService) =>
        {
            try
            {
                // Get conversation history from database
                var conversation = await conversationService.GetConversationAsync(request.ThreadId);
                if (conversation == null)
                {
                    return Results.BadRequest("Conversation not found");
                }

                // Save user message to database
                await conversationService.AddMessageAsync(request.ThreadId, request.Question, "user");

                // Create chat history from database messages
                var chatHistory = new ChatHistory();
                foreach (var dbMessage in conversation.Messages)
                {
                    if (dbMessage.Role == "user")
                        chatHistory.AddUserMessage(dbMessage.Content);
                    else if (dbMessage.Role == "assistant")
                        chatHistory.AddAssistantMessage(dbMessage.Content);
                }

                // Add the current user message
                chatHistory.AddUserMessage(request.Question);

                // Create thread with conversation history
                var thread = new ChatHistoryAgentThread(chatHistory, request.ThreadId);

                var responses = agent.InvokeAsync(request.Question, thread);

                // Collect all response messages - better approach for function calling scenarios
                var allResponses = new List<string>();
                await foreach (var agentResponse in responses)
                {
                    var content = agentResponse.Message.Content ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        allResponses.Add(content);
                    }
                }

                // Combine all responses into a single message for storage
                var finalResponse = string.Join("\n\n", allResponses);

                // If no content was generated, provide a fallback
                if (string.IsNullOrWhiteSpace(finalResponse))
                {
                    finalResponse = "No se pudo generar una respuesta.";
                }

                // Save assistant response to database
                await conversationService.AddMessageAsync(request.ThreadId, finalResponse, "assistant");

                return Results.Ok(new
                {
                    Message = finalResponse,
                    ConversationId = request.ThreadId,
                    ResponseCount = allResponses.Count
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error processing request: {ex.Message}");
            }
        });

        return app;
    }
}

public record AskQuestionRequest(string Question, string ThreadId);