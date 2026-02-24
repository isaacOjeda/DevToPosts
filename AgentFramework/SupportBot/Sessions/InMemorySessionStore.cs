using System.Collections.Concurrent;
using System.Text.Json;

namespace SupportBot.Sessions;

public class InMemorySessionStore
{
    private readonly ConcurrentDictionary<string, JsonElement> _sessions = new();

    public bool TryGetSession(string sessionId, out JsonElement session)
        => _sessions.TryGetValue(sessionId, out session);

    public void SaveSession(string sessionId, JsonElement session)
        => _sessions[sessionId] = session;

    public bool DeleteSession(string sessionId)
        => _sessions.TryRemove(sessionId, out _);
}
