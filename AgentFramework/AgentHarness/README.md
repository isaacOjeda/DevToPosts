# Agent Harness en Microsoft Agent Framework: cómo construir un agente de investigación autónomo en C#

## Introducción

Cuando empezamos a construir agentes con LLMs, tarde o temprano llegamos al mismo problema: un solo prompt no alcanza para tareas complejas. Necesitamos que el agente **itere**, llame herramientas varias veces, decida cuándo ya terminó y todo esto sin que nosotros tengamos que escribir el ciclo `while` a mano ni reinventar aprobaciones de herramientas, observabilidad o manejo de contexto.

Microsoft Agent Framework resuelve esto con el **Agent Harness**: una capa de runtime "batteries-included" que convierte un `IChatClient` en un agente capaz de ejecutar trabajo de larga duración (research, coding, análisis de datos, etc.), reutilizando los mismos bloques del framework (chat client, pipeline de chat, providers de contexto, middlewares) en vez de definir un runtime paralelo.

En este post explico los conceptos principales del Harness y cómo los apliqué en un ejemplo real: un **agente de investigación autónomo** que usa Tavily Search como herramienta y un `LoopEvaluator` para decidir cuándo detenerse.

### ¿Qué vamos a construir?

El ejemplo (`TopicResearchAgent`) es una consola donde le das un tema (por ejemplo, "arquitectura de Kafka") y el agente investiga por su cuenta: descompone el tema en preguntas, hace varias búsquedas en internet desde distintos ángulos (fundamentos, funcionamiento interno, alternativas, trade-offs), y recién cuando considera que tiene suficiente evidencia, compila un reporte técnico estructurado. Nosotros no le decimos cuántas búsquedas hacer ni en qué orden — eso lo decide el propio agente, iteración a iteración.

Ese es justo el motivo por el que el Harness encaja acá: no estamos resolviendo un problema de "una pregunta, una respuesta", sino uno de **trabajo autónomo de varios pasos** donde no sabemos de antemano cuántas iteraciones va a necesitar el modelo. Un `AIAgent` plano nos obligaría a escribir nosotros el `while` que decide cuándo volver a invocar al agente, cuándo pasarle feedback y cuándo cortar para no quedarnos en un loop infinito. El Harness ya trae esa pieza resuelta (el loop acotado con evaluadores de finalización), así que solo necesitamos configurar la condición de corte y el límite de seguridad, en vez de reinventar la orquestación del loop.

> 📌 El código completo de este ejemplo está en GitHub: [TopicResearchAgent](https://github.com/isaacOjeda/DevToPosts/tree/main/AgentFramework/AgentHarness/TopicResearchAgent). Aquí solo muestro las partes conceptualmente relevantes.

## Desarrollo

### ¿Qué problema resuelve el Harness?

Un `AIAgent` "plano" ejecuta una sola pasada: recibe un mensaje, eventualmente llama herramientas, y responde. Para tareas que requieren varias pasadas autónomas (investigar un tema, completar una lista de tareas, esperar un proceso en background) se necesita algo que re-invoque al agente hasta que se cumpla una condición de finalización, sin perder el control del contexto ni de las aprobaciones de herramientas.

El Harness compone esto a partir de piezas existentes del framework:

1. **Chat client**: conecta el agente al modelo.
2. **Chat pipeline**: agrega function calling, inyección de mensajes y persistencia de historial.
3. **Providers de contexto**: instrucciones, memoria, todos, modos de operación.
4. **Middlewares**: aprobación de herramientas, observabilidad y el **loop acotado** (opcional).
5. **UX de la aplicación**: quien consume el stream de respuestas.

El resultado sigue siendo un `AIAgent` normal — no es un runtime paralelo, es el mismo objeto que ya conoces del framework, pero con capacidades adicionales activadas por configuración.

### Creando un Harness Agent

Lo mínimo es un `IChatClient` y el método de extensión `AsHarnessAgent`:

```csharp
AIAgent agent = chatClient.AsHarnessAgent();
AgentResponse response = await agent.RunAsync("Plan a weekend trip to Seattle.");
```

Para configurarlo, `HarnessAgentOptions` permite separar dos niveles de instrucciones:

- `HarnessInstructions`: guía de "cómo comportarse como agente" (autonomía, disciplina al usar herramientas).
- `ChatOptions.Instructions`: instrucciones específicas del dominio del agente.

`HarnessInstructions` se antepone siempre a `ChatOptions.Instructions`, así que conviene pensarlas como capas: una gobierna el *comportamiento del agente*, la otra el *rol/tarea*.

### El loop acotado: `LoopEvaluators` y `CompletionMarkerLoopEvaluator`

Esta es la pieza central del ejemplo. El Harness no itera "para siempre": necesita un **evaluador de finalización** y un **límite máximo de iteraciones**.

```csharp
var harnessOptions = new HarnessAgentOptions
{
    // ...
    LoopEvaluators =
    [
        new CompletionMarkerLoopEvaluator("RESEARCH_COMPLETE")
    ],
    LoopAgentOptions = new LoopAgentOptions
    {
        MaxIterations = maxIterations
    }
};
```

- **`CompletionMarkerLoopEvaluator`** revisa la última respuesta del modelo buscando un marcador exacto (en este caso `RESEARCH_COMPLETE`). Mientras el marcador no aparezca, el loop sigue invocando al agente.
- **`LoopAgentOptions.MaxIterations`** es el límite duro: aunque el evaluador nunca vea el marcador, el loop no corre indefinidamente. Esto es clave — un evaluador de completitud puede fallar o el modelo puede "trabarse", así que Microsoft recomienda **siempre acotar los loops autónomos**.
- Si se configuran varios evaluadores, corren en orden y el loop se detiene solo cuando **todos** deciden no continuar.

Internamente, cuando configuras `LoopEvaluators` en `HarnessAgentOptions`, el Harness aplica un `LoopAgent` como el decorador más externo del agente. Es el mismo mecanismo que existe de forma standalone (`new LoopAgent(baseAgent, evaluator, options)`), solo que integrado al pipeline completo del Harness (aprobaciones, sesión, observabilidad).

Un detalle importante: el loop se detiene *antes* de evaluar la condición de finalización si una iteración devuelve una solicitud de aprobación de herramienta pendiente. Es decir, las aprobaciones "escapan" del loop en vez de quedar ocultas detrás de otra iteración automática.

### Aplicándolo al agente de investigación

En `TopicResearchAgent`, el objetivo es que el agente investigue un tema de forma autónoma, haciendo múltiples búsquedas web desde distintos ángulos antes de compilar un reporte final. La estructura queda así:

```csharp
var harnessOptions = new HarnessAgentOptions
{
    Name = "DeepResearchAgent",
    HarnessInstructions = """
        You are an elite, autonomous technical researcher and architect.
        ...
        Only emit the final marker RESEARCH_COMPLETE when your structured deep-dive report is fully compiled.
        """,
    ChatOptions = new ChatOptions
    {
        Instructions = """
            # Autonomous Research Protocol
            ...
            """,
        Tools = [ AIFunctionFactory.Create(tavilyTool.SearchWebAsync, "search_web", "...") ]
    },
    LoopEvaluators = [ new CompletionMarkerLoopEvaluator("RESEARCH_COMPLETE") ],
    LoopAgentOptions = new LoopAgentOptions { MaxIterations = maxIterations }
};

AIAgent agent = chatClient.AsHarnessAgent(harnessOptions);
```

Algunas decisiones de diseño que vale la pena resaltar:

- **`HarnessInstructions` vs `ChatOptions.Instructions`**: la primera define el *comportamiento autónomo* del agente (no generar resúmenes superficiales, usar la herramienta deliberadamente); la segunda define el *protocolo de trabajo* concreto (deconstruir, investigar, sintetizar en un reporte con secciones fijas).
- **La herramienta (`search_web`)** se registra con `AIFunctionFactory.Create`, envolviendo `TavilySearchTool.SearchWebAsync`, que llama a la API de Tavily y devuelve resultados formateados como texto plano para que el modelo los consuma.
- **El marcador de finalización** (`RESEARCH_COMPLETE`) está explícitamente pedido en las instrucciones del Harness, para que el modelo sepa exactamente cuándo debe emitirlo.
- **`MaxIterations`** viene de configuración (`Agent:MaxIterations`, default `6`), así el límite es ajustable sin recompilar.

El consumo final es igual de simple que con cualquier `AIAgent`, usando streaming:

```csharp
await foreach (var update in agent.RunStreamingAsync(prompt))
{
    Console.Write(update.Text);
}
```

Cada iteración del loop se emite como parte del stream, así que en consola se ve al agente "pensando en voz alta" — buscando, evaluando, buscando de nuevo — hasta que decide que terminó.

### Tavily: la herramienta que le da ojos al agente

Todo el loop de investigación depende de que el agente pueda buscar información real en internet, y ahí es donde entra **Tavily**: una API de búsqueda pensada específicamente para ser consumida por agentes/LLMs (a diferencia de un motor de búsqueda tradicional orientado a humanos). Devuelve resultados ya "listos para el modelo" y, opcionalmente, una respuesta sintetizada además de los links.

`TavilySearchTool` es un wrapper delgado sobre esa API:

```csharp
var requestBody = new TavilySearchRequest
{
    ApiKey = _apiKey,
    Query = query,
    SearchDepth = "advanced",
    IncludeAnswer = true,
    MaxResults = 5
};

using var response = await _httpClient.PostAsJsonAsync(
    "https://api.tavily.com/search",
    requestBody,
    cancellationToken);
```

Un par de detalles que hacen que esta herramienta funcione bien dentro del loop del Harness:

- **`SearchDepth = "advanced"`**: le pide a Tavily un análisis más profundo (no solo snippets superficiales), justo lo que necesita el agente para producir un reporte técnico con sustancia.
- **`IncludeAnswer = true`**: Tavily devuelve un resumen directo además de las fuentes, lo cual le da al modelo una síntesis rápida antes de leer los resultados completos.
- **Salida en texto plano**: la herramienta arma un string con el resumen y cada fuente (título, URL, contenido), porque el modelo consume el resultado de la función como texto — no como un objeto estructurado.

Esta herramienta es la que se registra como `search_web` con `AIFunctionFactory.Create` (visto en la sección anterior). Sin ella, el `LoopEvaluator` seguiría pidiendo iteraciones, pero el agente no tendría con qué alimentarlas: Tavily es lo que convierte cada vuelta del loop en una búsqueda real y no en especulación del modelo.

## Conclusión

El Agent Harness no reemplaza los conceptos que ya conocemos de Agent Framework (chat clients, tools, `AIAgent`); los **compone** para darnos capacidades de nivel superior sin escribir infraestructura desde cero. Para este tipo de agentes autónomos, lo más valioso es la combinación de:

- Un **loop acotado** (`LoopEvaluators` + `MaxIterations`) que evita ejecuciones infinitas.
- Una condición de finalización explícita y verificable (`CompletionMarkerLoopEvaluator`), en vez de depender de heurísticas implícitas.
- Separación clara entre instrucciones de comportamiento (`HarnessInstructions`) e instrucciones de dominio (`ChatOptions.Instructions`).

Con pocas líneas de configuración se obtiene un agente que investiga un tema de forma iterativa y se detiene solo cuando genuinamente terminó, en vez de en un número fijo de turnos.

## Referencias

- [Agent Harness — Microsoft Learn](https://learn.microsoft.com/en-us/agent-framework/concepts/harness?pivots=programming-language-csharp)
- [Agent Looping — Microsoft Learn](https://learn.microsoft.com/en-us/agent-framework/agents/looping?pivots=programming-language-csharp)
