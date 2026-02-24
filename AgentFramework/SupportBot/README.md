# Primeros pasos con Microsoft Agent Framework: construyendo un chatbot de soporte con C#

## Introducción

Microsoft lleva años apostando por herramientas para construir aplicaciones con IA: primero llegó **Semantic Kernel**, después **AutoGen**, y ahora ambos convergen en su sucesor directo: **Microsoft Agent Framework (MAF)**. Creado por los mismos equipos, MAF unifica lo mejor de ambos mundos — la estabilidad y características enterprise de Semantic Kernel con las abstracciones simples para agentes multi-turno de AutoGen — y agrega nuevas capacidades como workflows basados en grafos y un sistema robusto de manejo de estado.

Al momento de escribir este artículo, MAF está en **public preview**, disponible para .NET y Python bajo licencia MIT.

En este artículo vamos a aprender los conceptos fundamentales del framework y a construir algo concreto: una API con ASP.NET Core 10 que expone un chatbot de soporte técnico capaz de responder preguntas basándose en documentación Markdown interna, manteniendo el contexto de la conversación entre mensajes.

> El código completo está disponible en [GitHub](#).

---

## Desarrollo

### ¿Qué es Microsoft Agent Framework?

MAF gira en torno a dos conceptos centrales que vale la pena entender antes de escribir código:

**Agents** son sistemas que usan un LLM para procesar entradas, tomar decisiones, llamar herramientas y generar respuestas. Son ideales cuando la tarea es dinámica y no puedes predecir de antemano exactamente qué pasos se van a necesitar — como una conversación de soporte donde el usuario puede preguntar cualquier cosa.

**Workflows** son secuencias de pasos definidas explícitamente, conectadas en un grafo. Son la elección correcta cuando el flujo es predecible y necesitas control determinista sobre la ejecución — como un pipeline de procesamiento de datos o un flujo de aprobaciones.

La documentación oficial tiene una regla de oro que me parece honesta y útil: *"If you can write a function to handle the task, do that instead of using an AI agent."* No todo necesita un agente. En este artículo construimos un agente porque la naturaleza conversacional e impredecible del soporte técnico es exactamente el caso de uso para el que están diseñados.

### Los 5 conceptos clave

El tutorial oficial de Microsoft Learn organiza el aprendizaje en 5 pasos progresivos. Aquí los resumo en mis propias palabras antes de ver el código:

**1. Tu primer agente.** Un `AIAgent` se crea a partir de un cliente de chat (Azure OpenAI, OpenAI, etc.) con instrucciones y un nombre. Es stateless por diseño — el mismo agente puede atender múltiples conversaciones en paralelo. Correrlo es tan simple como `await agent.RunAsync("pregunta")`.

**2. Tools.** Cualquier método C# decorado con `[Description]` se puede convertir en una herramienta que el agente puede llamar cuando lo necesite, usando `AIFunctionFactory.Create()`. El LLM decide autónomamente cuándo y con qué argumentos invocarla. Esto es lo que le da al agente la capacidad de actuar sobre el mundo real.

**3. Multi-turn conversations.** Como el agente es stateless, el historial de la conversación vive en un `AgentSession`. Se crea con `agent.CreateSessionAsync()` y se pasa en cada llamada. El agente recuerda todo lo que ocurrió en esa sesión.

**4. Memory y Persistencia.** La sesión es serializable a `JsonElement`. Esto permite guardarla en cualquier almacenamiento (memoria, base de datos, Redis) y reconstruirla después con `agent.DeserializeSessionAsync()`. Para un chat de soporte, esto significa que el usuario puede retomar una conversación donde la dejó, incluso si el servidor se reinició.

**5. Workflows.** Para orquestar múltiples agentes o pasos en secuencias definidas, se usa `WorkflowBuilder`. Se definen `Executors` (unidades de procesamiento) y se conectan con `Edges`. En este artículo no los implementamos porque no añaden valor real al caso de uso — pero al final menciono cuándo sí tendría sentido usarlos.

### El ejemplo: SupportBot

El escenario es simple: una empresa tiene documentación interna en archivos `.md` — guías de usuario, FAQs, manuales de módulos. En lugar de que los empleados busquen manualmente en esos archivos, un chatbot lee la documentación y responde en lenguaje natural, manteniendo el hilo de la conversación.

Deliberadamente dejé fuera RAG y embeddings. La búsqueda por keywords en archivos planos es suficiente para demostrar cómo funciona MAF, y mantiene el ejemplo enfocado en el framework, no en infraestructura de búsqueda vectorial.

La estructura del proyecto es la siguiente:

```
SupportBot/
├── Program.cs
├── appsettings.json
├── Docs/
│   ├── accesos.md
│   ├── facturacion.md
│   └── reportes.md
├── Agents/
│   └── SupportAgentFactory.cs
├── Tools/
│   └── DocumentationTool.cs
├── Models/
│   ├── ChatRequest.cs
│   └── ChatResponse.cs
└── Sessions/
    └── InMemorySessionStore.cs
```

### Instalación

```bash
dotnet new webapi -n SupportBot
cd SupportBot

dotnet add package Azure.AI.OpenAI --prerelease
dotnet add package Azure.Identity
dotnet add package Microsoft.Agents.AI --prerelease
dotnet add package Microsoft.Agents.AI.OpenAI --prerelease
dotnet add package Microsoft.Extensions.AI --prerelease
```

Todos los paquetes de `Microsoft.Agents.AI` están en prerelease — el flag es necesario.

### La Tool: DocumentationTool

Aquí está el corazón del ejemplo. `DocumentationTool` es la herramienta que el agente llamará cuando necesite buscar información para responder al usuario.

```csharp
using System.ComponentModel;

namespace SupportBot.Tools;

public class DocumentationTool
{
    private readonly string _docsPath;

    public DocumentationTool(string docsPath)
    {
        _docsPath = docsPath;
    }

    [Description("Busca en la documentación interna del sistema información sobre un tema específico. Úsala siempre antes de responder una pregunta del usuario.")]
    public string GetDocumentation(
        [Description("El tema o concepto sobre el que buscar. Por ejemplo: 'accesos', 'facturación', 'reportes', 'contraseña', etc.")]
        string topic)
    {
        if (!Directory.Exists(_docsPath))
            return "No hay documentación disponible en este momento.";

        var files = Directory.GetFiles(_docsPath, "*.md");

        if (files.Length == 0)
            return "No hay documentación disponible en este momento.";

        var keywords = topic.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var matchingFiles = files.Where(file =>
        {
            var fileName = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
            var content = File.ReadAllText(file).ToLowerInvariant();
            return keywords.Any(k => fileName.Contains(k) || content.Contains(k));
        }).ToList();

        var filesToRead = matchingFiles.Count > 0 ? matchingFiles : files.ToList();

        var builder = new System.Text.StringBuilder();
        foreach (var file in filesToRead)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var content = File.ReadAllText(file);
            builder.AppendLine($"=== {fileName.ToUpperInvariant()} ===");
            builder.AppendLine(content);
            builder.AppendLine();
        }

        return builder.ToString();
    }
}
```

Hay dos detalles importantes aquí. El primero es que los atributos `[Description]` no son decorativos — MAF los usa para construir el schema que el LLM recibe, y de su calidad depende que el modelo entienda bien cuándo y cómo llamar la herramienta. El segundo es la estrategia de fallback: si no encuentra archivos que hagan match con el tema, retorna todos los documentos. Es una decisión pragmática — preferimos que el agente tenga demasiado contexto a que se quede sin información para responder.

### El Agente: SupportAgentFactory

```csharp
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using SupportBot.Tools;

namespace SupportBot.Agents;

public class SupportAgentFactory
{
    private readonly AIAgent _agent;

    public SupportAgentFactory(IConfiguration configuration, IWebHostEnvironment env)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"]
            ?? throw new InvalidOperationException("AzureOpenAI:Endpoint no configurado");
        var deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o-mini";
        var apiKey = configuration["AzureOpenAI:ApiKey"]
            ?? throw new InvalidOperationException("AzureOpenAI:ApiKey no configurado");

        var docsPath = Path.Combine(env.ContentRootPath, "Docs");
        var docTool = new DocumentationTool(docsPath);

        var instructions = """
            Eres un asistente de soporte técnico interno. Tu única fuente de información
            es la documentación del sistema que puedes consultar con la herramienta GetDocumentation.

            Reglas:
            - SIEMPRE consulta la documentación antes de responder cualquier pregunta.
            - Responde de forma clara, concisa y en el mismo idioma que el usuario.
            - Si la documentación no contiene la respuesta, dilo claramente.
            - No inventes información que no esté en la documentación.
            - Si el usuario saluda o hace preguntas generales, responde amablemente y pregunta en qué puedes ayudar.
            - No respondas preguntas fuera del contexto del sistema.
            """;

        _agent = new AzureOpenAIClient(
                new Uri(endpoint),
                new System.ClientModel.ApiKeyCredential(apiKey))
            .GetChatClient(deploymentName)
            .AsIChatClient()
            .AsAIAgent(
                instructions: instructions,
                name: "SupportAgent",
                tools: [AIFunctionFactory.Create(docTool.GetDocumentation)]
            );
    }

    public AIAgent Agent => _agent;
}
```

El flujo de construcción es una cadena: `AzureOpenAIClient` → `GetChatClient()` → `AsIChatClient()` → `AsAIAgent()`. El último paso es donde se convierten las tools registradas y las instrucciones en la configuración que el agente usará en cada conversación.

Nótese que el agente se construye una sola vez y se registra como singleton. Como es stateless, puede atender todas las conversaciones concurrentes sin problema — el estado de cada conversación vive en su propia `AgentSession`.

### Manejo de Sesiones: InMemorySessionStore

```csharp
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
```

El store guarda sesiones como `JsonElement` — el formato en que MAF las serializa nativamente. `ConcurrentDictionary` garantiza thread-safety sin necesidad de locks manuales. Para producción, esto se reemplazaría por Redis o una base de datos, pero la interfaz del store no cambiaría.

### Los Endpoints: Program.cs

```csharp
using System.Text.Json;
using Microsoft.Agents.AI;
using SupportBot.Agents;
using SupportBot.Models;
using SupportBot.Sessions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SupportAgentFactory>();
builder.Services.AddSingleton<InMemorySessionStore>();

var app = builder.Build();

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

    AgentSession session;
    if (sessionStore.TryGetSession(request.SessionId, out var serializedSession))
    {
        session = await agent.DeserializeSessionAsync(serializedSession);
    }
    else
    {
        session = await agent.CreateSessionAsync();
    }

    var response = await agent.RunAsync(request.Message, session);

    var updatedSession = await agent.SerializeSessionAsync(session);
    sessionStore.SaveSession(request.SessionId, updatedSession);

    return Results.Ok(new ChatResponse(request.SessionId, response.Text));
});

app.MapDelete("/chat/{sessionId}", (
    string sessionId,
    InMemorySessionStore sessionStore) =>
{
    sessionStore.DeleteSession(sessionId);
    return Results.NoContent();
});

app.Run();
```

El endpoint `POST /chat` sigue un patrón claro: cargar o crear sesión → ejecutar el agente → serializar y guardar la sesión actualizada → retornar la respuesta. El `sessionId` lo genera el cliente (puede ser un GUID) y lo manda en cada request para mantener el hilo de la conversación.

Un detalle de la versión RC1: `SerializeSessionAsync` es async, a diferencia de lo que indica la documentación inicial del framework.

### El agente en acción

Con la API corriendo, así se ve una conversación multi-turn real:

**Primer mensaje:**
```json
POST /chat
{
  "sessionId": "usr-42",
  "message": "Hola, no puedo entrar al sistema, olvidé mi contraseña"
}
```
```json
{
  "sessionId": "usr-42",
  "reply": "¡Hola! Para resetear tu contraseña sigue estos pasos: ve a la pantalla de login y haz clic en '¿Olvidaste tu contraseña?', ingresa tu correo corporativo y recibirás un enlace válido por 24 horas. Si no recibes el correo en 10 minutos, revisa tu carpeta de spam o contacta a TI."
}
```

**Mensaje de seguimiento (mismo sessionId):**
```json
POST /chat
{
  "sessionId": "usr-42",
  "message": "¿Y si tampoco recuerdo mi correo corporativo?"
}
```
```json
{
  "sessionId": "usr-42",
  "reply": "En ese caso, la cuenta puede quedar bloqueada tras 5 intentos fallidos. Te recomiendo contactar directamente al equipo de soporte en soporte@empresa.com o llamar al ext. 100 para que puedan verificar tu identidad y ayudarte a recuperar el acceso."
}
```

El agente recuerda que estamos hablando de un problema de acceso y responde en contexto, sin necesidad de que el usuario repita la situación.

### ¿Cuándo agregarías Workflows aquí?

En este ejemplo los Workflows no añaden valor — el flujo conversacional es impredecible por naturaleza y el agente lo maneja bien solo. Pero hay escenarios donde sí tendría sentido introducirlos:

Si quisieras **clasificar automáticamente la intención** antes de responder (¿es una pregunta de accesos, facturación o reportes?) y enrutar a agentes especializados según el tema, un Workflow con un executor de clasificación seguido de ejecutores especializados sería la arquitectura correcta.

Si quisieras **escalar a un humano** cuando el agente no encuentra respuesta, un Workflow con un paso de "human-in-the-loop" integrado en el grafo lo haría de forma limpia y auditable.

La regla es simple: si puedes predecir los pasos, usa un Workflow. Si la conversación es abierta e impredecible, deja que el agente decida.

---

## Conclusión

Microsoft Agent Framework simplifica genuinamente la construcción de agentes conversacionales en .NET. La abstracción de `AIAgent`, el manejo de sesiones serializable y el sistema de tools via atributos `[Description]` permiten tener algo funcional con muy poco código de infraestructura — lo que queda es lógica de negocio real.

El hecho de que aún esté en preview se nota en algunos detalles: la API cambia entre versiones (como el caso de `SerializeSessionAsync` que en RC1 es async), y la documentación a veces no refleja el estado actual del código. Dicho esto, para proyectos nuevos donde el timeline lo permite, ya vale la pena apostar por él en lugar de Semantic Kernel — es el camino hacia adelante según el propio equipo de Microsoft.

Los siguientes pasos naturales para este proyecto serían reemplazar el `InMemorySessionStore` por Redis o SQL Server para persistencia real, agregar autenticación al endpoint, e integrar un frontend — pero eso es material para otro artículo.

---

## Referencias

- [Microsoft Agent Framework Overview](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview) — Microsoft Learn
- [Get Started: Steps 1–5](https://learn.microsoft.com/en-us/agent-framework/get-started/) — Microsoft Learn
- [Repositorio oficial con samples](https://github.com/microsoft/agent-framework) — GitHub
- [Anuncio oficial: Semantic Kernel y Microsoft Agent Framework](https://devblogs.microsoft.com/semantic-kernel/semantic-kernel-and-microsoft-agent-framework/) — Microsoft Dev Blog
- [Migration Guide from Semantic Kernel](https://learn.microsoft.com/en-us/agent-framework/migration-guide/from-semantic-kernel/) — Microsoft Learn