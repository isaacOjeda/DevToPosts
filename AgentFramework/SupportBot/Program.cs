using System.Text.Json;
using Microsoft.Agents.AI;
using SupportBot.Agents;
using SupportBot.Models;
using SupportBot.Sessions;

var builder = WebApplication.CreateBuilder(args);

// DI
builder.Services.AddSingleton<SupportAgentFactory>();
builder.Services.AddSingleton<InMemorySessionStore>();
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();

// POST /chat
app.MapPost("/chat", async (
    ChatRequest request,
    SupportAgentFactory agentFactory,
    InMemorySessionStore sessionStore) =>
{
    if (string.IsNullOrWhiteSpace(request.SessionId))
        return Results.BadRequest("SessionId es requerido.");

    if (string.IsNullOrWhiteSpace(request.Message))
        return Results.BadRequest("Message es requerido.");

    var agent = agentFactory.Agent;

    // Cargar o crear sesión
    AgentSession session;
    if (sessionStore.TryGetSession(request.SessionId, out var serializedSession))
    {
        session = await agent.DeserializeSessionAsync(serializedSession);
    }
    else
    {
        session = await agent.CreateSessionAsync();
    }

    // Ejecutar el agente
    var response = await agent.RunAsync(request.Message, session);

    // Serializar y guardar la sesión actualizada (SerializeSessionAsync es async en rc1)
    var updatedSession = await agent.SerializeSessionAsync(session);
    sessionStore.SaveSession(request.SessionId, updatedSession);

    return Results.Ok(new ChatResponse(request.SessionId, response.Text));
});

// DELETE /chat/{sessionId}
app.MapDelete("/chat/{sessionId}", (
    string sessionId,
    InMemorySessionStore sessionStore) =>
{
    sessionStore.DeleteSession(sessionId);
    return Results.NoContent();
});

app.MapRazorPages();

app.Run();