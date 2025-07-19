# Sistema de Gestión de Facturas con Semantic Kernel y SQLite

Este proyecto demuestra cómo crear un sistema completo de gestión de facturas usando **Semantic Kernel**, **Entity Framework Core** y **SQLite** para persistir datos.

## ?? Características

### ?? Gestión de Facturas
- ? Verificar estado de pago de facturas específicas
- ?? Crear prefacturas (borradores) para clientes
- ?? Consultar facturas pendientes de pago
- ?? Marcar facturas como pagadas
- ?? Gestión de información de clientes
- ?? Identificación automática de facturas vencidas

### ?? Inteligencia Artificial
- **Semantic Kernel Agents** para procesamiento de lenguaje natural
- **Function Calling** automático para ejecutar acciones específicas
- **Persistencia de conversaciones** en SQLite
- **Respuestas contextuales** con historial de conversación

## ??? Arquitectura

```
SemanticKernelLearning04/
??? Models/
?   ??? Conversation.cs          # Modelo para conversaciones
?   ??? ConversationMessage.cs   # Mensajes de conversación
?   ??? Customer.cs              # Modelo de clientes
?   ??? Invoice.cs               # Modelo de facturas
??? Data/
?   ??? ConversationDbContext.cs # DbContext con EF Core
??? Services/
?   ??? ConversationService.cs   # Servicio de conversaciones
?   ??? InvoiceService.cs        # Servicio de facturas
??? Plugins/
    ??? InvoicesPlugin.cs        # Plugin con funciones de facturas
```

## ?? Modelos de Datos

### Invoice (Factura)
- **InvoiceNumber**: Número único de factura
- **Customer**: Cliente asociado
- **Description**: Descripción del servicio
- **Amount**: Monto de la factura
- **Status**: Estado (Borrador, Enviada, Pagada, Vencida, Cancelada)
- **DueDate**: Fecha de vencimiento
- **PaidDate**: Fecha de pago (si aplica)

### Customer (Cliente)
- **Name**: Nombre completo
- **Email**: Email único
- **Phone**: Teléfono
- **Address**: Dirección

## ??? Configuración

### 1. Dependencias
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
<PackageReference Include="Microsoft.SemanticKernel" Version="1.60.0" />
<PackageReference Include="Microsoft.SemanticKernel.Agents.Core" Version="1.60.0" />
```

### 2. Base de Datos
- **SQLite** para almacenamiento local
- **EF Core** para mapeo objeto-relacional
- **Datos de muestra** se crean automáticamente al iniciar

### 3. Configuración del Agent
```csharp
var agent = new ChatCompletionAgent()
{
    Name = "Asistente",
    Instructions = """
                   Eres un asistente de abogados en una notaría pública. 
                   Tu tarea es ayudar a los abogados a gestionar y verificar el estado de las facturas.
                   """,
    Kernel = agentKernel,
    Arguments = new KernelArguments(
        new OpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        })
};
```

## ?? Funciones Disponibles

### 1. VerifyPaymentAsync
Verifica el estado de una factura específica.
```
"¿Puedes verificar el estado de pago de la factura INV-202412-0001?"
```

### 2. CreateInvoiceDraftAsync
Crea una nueva prefactura para un cliente.
```
"Crea una prefactura para juan.perez@email.com por servicios de registro de marca por $2,000 pesos"
```

### 3. GetUnpaidInvoicesAsync
Obtiene todas las facturas sin pagar.
```
"Muéstrame todas las facturas sin pagar"
```

### 4. MarkInvoiceAsPaidAsync
Marca una factura como pagada.
```
"Marca la factura INV-202412-0002 como pagada"
```

### 5. GetCustomerInfoAsync
Obtiene información detallada de un cliente.
```
"Dame información detallada del cliente maria.garcia@email.com"
```

## ?? Datos de Muestra

El sistema incluye datos de muestra que se crean automáticamente:

### Clientes
- Juan Pérez (juan.perez@email.com)
- María García (maria.garcia@email.com)
- Carlos López (carlos.lopez@email.com)
- Ana Rodríguez (ana.rodriguez@email.com)

### Facturas
- **INV-202412-0001**: Servicios legales - Compraventa (PAGADA)
- **INV-202412-0002**: Constitución de sociedad (ENVIADA)
- **INV-202412-0003**: Testamento público abierto (VENCIDA)
- **INV-202412-0004**: Poder notarial (BORRADOR)
- **INV-202412-0005**: Cancelación de hipoteca (VENCIDA)

## ?? Pruebas

Utiliza el archivo `SemanticKernelLearning04.http` para probar todas las funcionalidades:

1. **Iniciar conversación**: Crear un nuevo hilo de conversación
2. **Verificar facturas**: Consultar estado de facturas específicas
3. **Gestionar pagos**: Marcar facturas como pagadas
4. **Crear facturas**: Generar nuevas prefacturas
5. **Consultas complejas**: Usar lenguaje natural para consultas avanzadas

## ?? Características Destacadas

### Respuestas Visuales
Las respuestas incluyen emojis y formato para mejor legibilidad:
```
?? **Factura: INV-202412-0001**
?? Cliente: Juan Pérez
?? Descripción: Servicios legales - Compraventa
?? Monto: $1,500.00
?? Fecha de vencimiento: 15/12/2024
? Estado: Pagada
?? Fecha de pago: 20/12/2024
```

### Function Calling Inteligente
El agente selecciona automáticamente la función correcta basándose en la consulta del usuario, sin necesidad de especificar parámetros técnicos.

### Persistencia Completa
- Conversaciones guardadas en SQLite
- Historial completo de interacciones
- Datos de facturas persistentes entre sesiones

## ?? Conceptos Aprendidos

1. **Semantic Kernel Agents**: Configuración y uso de agentes inteligentes
2. **Function Calling**: Creación de plugins con funciones específicas
3. **Entity Framework Core**: Mapeo de objetos y relaciones
4. **SQLite Integration**: Persistencia de datos local
5. **Dependency Injection**: Inyección de servicios en plugins
6. **Async/Await Patterns**: Manejo asíncrono de datos
7. **Business Logic**: Implementación de reglas de negocio

Este ejemplo es perfecto para demostrar cómo combinar IA conversacional con persistencia de datos real en aplicaciones empresariales.

---

# Tutorial: Sistema de Gestión de Facturas con Semantic Kernel y Agentes de IA

## ?? Introducción

Este tutorial te guiará paso a paso para crear un sistema completo de gestión de facturas utilizando **Microsoft Semantic Kernel** y **agentes conversacionales**. El proyecto demuestra cómo integrar inteligencia artificial con APIs REST para crear un asistente que puede manejar operaciones de negocio complejas usando lenguaje natural.

### ¿Qué aprenderás?

- **Configuración de Semantic Kernel**: Integración con Azure OpenAI
- **Creación de Agentes Conversacionales**: ChatCompletionAgent para procesamiento de lenguaje natural
- **Function Calling**: Plugins personalizados que el agente puede ejecutar automáticamente
- **API Endpoints**: Exposición de funcionalidades a través de REST APIs
- **Persistencia de Conversaciones**: Almacenamiento del historial en SQLite

### ¿Qué construiremos?

Un sistema completo donde los usuarios pueden:
- Verificar estados de facturas usando lenguaje natural
- Crear nuevas facturas con comandos simples
- Consultar información de clientes
- Gestionar pagos y estados de facturas
- Mantener conversaciones contextuales con historial persistente

---

## ?? Desarrollo

### 1. Configuración Inicial del Proyecto

#### 1.1 Dependencias Necesarias

Primero, necesitamos instalar los paquetes NuGet requeridos para Semantic Kernel y Entity Framework:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
<PackageReference Include="Microsoft.SemanticKernel" Version="1.60.0" />
<PackageReference Include="Microsoft.SemanticKernel.Agents.Core" Version="1.60.0" />
```

#### 1.2 Configuración de Azure OpenAI

En `appsettings.json`, configura tu instancia de Azure OpenAI:

```json
{
  "AzureOpenAI": {
    "DeploymentName": "gpt-4",
    "ApiKey": "tu-api-key-aquí",
    "Endpoint": "https://tu-instancia.openai.azure.com/",
    "ModelId": "gpt-4"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=conversations.db"
  }
}
```

### 2. Creación del Plugin de Facturas

#### 2.1 Estructura del Plugin

Un plugin en Semantic Kernel es una clase que contiene métodos decorados con `[KernelFunction]`. Estos métodos pueden ser invocados automáticamente por el agente:

```csharp
public class InvoicesPlugin
{
    private readonly InvoiceService _invoiceService;

    public InvoicesPlugin(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [KernelFunction]
    [Description("Verifica el estado de pago de una factura específica usando su número de factura.")]
    public async Task<string> VerifyPaymentAsync(
        [Description("Número de la factura a verificar (ej: INV-202412-0001)")] 
        string numeroFactura)
    {
        // Implementación del método
    }
}
```

#### 2.2 Funciones Clave del Plugin

**Verificación de Pagos:**
```csharp
[KernelFunction]
[Description("Verifica el estado de pago de una factura específica usando su número de factura.")]
public async Task<string> VerifyPaymentAsync([Description("Número de la factura a verificar")] string numeroFactura)
{
    try
    {
        var invoice = await _invoiceService.GetInvoiceByNumberAsync(numeroFactura);
        
        if (invoice == null)
        {
            return $"? No se encontró ninguna factura con el número: {numeroFactura}";
        }

        var result = new StringBuilder();
        result.AppendLine($"?? **Factura: {invoice.InvoiceNumber}**");
        result.AppendLine($"?? Cliente: {invoice.Customer.Name}");
        result.AppendLine($"?? Monto: ${invoice.Amount:F2}");
        result.AppendLine($"?? Estado: {GetStatusText(invoice.Status)}");

        return result.ToString();
    }
    catch (Exception ex)
    {
        return $"? Error al verificar la factura: {ex.Message}";
    }
}
```

**Creación de Facturas:**
```csharp
[KernelFunction]
[Description("Realiza una prefactura (borrador) para un cliente específico.")]
public async Task<string> CreateInvoiceDraftAsync(
    [Description("Email del cliente para la prefactura")] string clienteEmail,
    [Description("Descripción del servicio o trabajo realizado")] string descripcion,
    [Description("Monto de la factura en pesos")] decimal monto,
    [Description("Días hasta el vencimiento (opcional, por defecto 30 días)")] int diasVencimiento = 30)
{
    // Implementación completa en el archivo del proyecto
}
```

### 3. Configuración del Agente Conversacional

#### 3.1 Registro de Servicios

Utilizamos la inyección de dependencias para configurar todos los servicios necesarios:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar Azure OpenAI desde appsettings
        var azureOpenAIConfig = configuration.GetSection("AzureOpenAI");
        services.AddAzureOpenAIChatCompletion(
            deploymentName: azureOpenAIConfig["DeploymentName"]!,
            apiKey: azureOpenAIConfig["ApiKey"]!,
            endpoint: azureOpenAIConfig["Endpoint"]!,
            modelId: azureOpenAIConfig["ModelId"]!
        );

        services.AddTransient(serviceProvider => new Kernel(serviceProvider));
        
        return services;
    }
}
```

#### 3.2 Creación del Agente

El agente se configura como un servicio con clave para poder inyectarlo específicamente:

```csharp
public static IServiceCollection AddAgents(this IServiceCollection services)
{
    services.AddKeyedTransient<ChatCompletionAgent>("AssistantAgent", (sp, _) =>
    {
        var kernel = sp.GetRequiredService<Kernel>();
        var agentKernel = kernel.Clone();

        // Registrar el InvoicesPlugin con inyección de dependencias
        var invoiceService = sp.GetRequiredService<InvoiceService>();
        agentKernel.ImportPluginFromObject(new InvoicesPlugin(invoiceService));

        var agent = new ChatCompletionAgent()
        {
            Name = "Asistente",
            Instructions = """
                       Eres un asistente de abogados en una notaría pública. 
                       Tu tarea es ayudar a los abogados a gestionar y verificar el estado de las facturas.
                       
                       Funciones disponibles:
                       - Verificar el estado de pago de facturas específicas
                       - Crear prefacturas para clientes
                       - Consultar facturas pendientes de pago
                       - Marcar facturas como pagadas
                       - Consultar información de clientes
                       
                       Siempre proporciona información clara y detallada sobre el estado de las facturas.
                       Usa emojis para hacer las respuestas más visuales y fáciles de entender.
                       """,
            Kernel = agentKernel,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })
        };

        return agent;
    });
    
    return services;
}
```

**Puntos clave de la configuración:**

1. **Instructions**: Define el comportamiento y personalidad del agente
2. **FunctionChoiceBehavior.Auto()**: Permite al agente decidir automáticamente qué funciones llamar
3. **Kernel Clone**: Cada agente tiene su propia instancia del kernel con sus plugins específicos

### 4. Implementación de Endpoints REST

#### 4.1 Estructura de Endpoints

Los endpoints proporcionan la interfaz REST para interactuar con el agente:

```csharp
public static class AgentEndpoints
{
    public static WebApplication MapAgentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/agent")
            .WithTags("Agent")
            .WithOpenApi();

        // Endpoint para iniciar conversación
        group.MapPost("/start", async (ConversationService conversationService) =>
        {
            var conversationId = await conversationService.CreateConversationAsync();
            return Results.Ok(new { ConversationId = conversationId });
        });

        // Endpoint principal para interactuar con el agente
        group.MapPost("/", async (AskQuestionRequest request,
            [FromKeyedServices("AssistantAgent")] ChatCompletionAgent agent,
            ConversationService conversationService) =>
        {
            // Implementación del endpoint principal
        });

        return app;
    }
}
```

#### 4.2 Endpoint Principal de Conversación

Este es el corazón del sistema, donde se maneja la interacción con el agente:

```csharp
group.MapPost("/", async (AskQuestionRequest request,
    [FromKeyedServices("AssistantAgent")] ChatCompletionAgent agent,
    ConversationService conversationService) =>
{
    try
    {
        // 1. Obtener historial de conversación desde la base de datos
        var conversation = await conversationService.GetConversationAsync(request.ThreadId);
        if (conversation == null)
        {
            return Results.BadRequest("Conversation not found");
        }

        // 2. Guardar mensaje del usuario en la base de datos
        await conversationService.AddMessageAsync(request.ThreadId, request.Question, "user");

        // 3. Crear historial de chat desde mensajes de base de datos
        var chatHistory = new ChatHistory();
        foreach (var dbMessage in conversation.Messages)
        {
            if (dbMessage.Role == "user")
                chatHistory.AddUserMessage(dbMessage.Content);
            else if (dbMessage.Role == "assistant")
                chatHistory.AddAssistantMessage(dbMessage.Content);
        }

        // 4. Agregar el mensaje actual del usuario
        chatHistory.AddUserMessage(request.Question);

        // 5. Crear hilo de conversación con el historial
        var thread = new ChatHistoryAgentThread(chatHistory, request.ThreadId);

        // 6. Invocar al agente y recopilar respuestas
        var responses = agent.InvokeAsync(request.Question, thread);

        var allResponses = new List<string>();
        await foreach (var agentResponse in responses)
        {
            var content = agentResponse.Message.Content ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(content))
            {
                allResponses.Add(content);
            }
        }

        // 7. Combinar todas las respuestas en un solo mensaje
        var finalResponse = string.Join("\n\n", allResponses);

        // 8. Guardar respuesta del asistente en la base de datos
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
```

**Flujo de procesamiento explicado:**

1. **Recuperación del Contexto**: Se obtiene el historial completo de la conversación
2. **Persistencia del Input**: El mensaje del usuario se guarda inmediatamente
3. **Construcción del Contexto**: Se recrea el historial para el agente
4. **Invocación del Agente**: El agente procesa el mensaje con todo el contexto
5. **Function Calling**: El agente puede llamar múltiples funciones si es necesario
6. **Agregación de Respuestas**: Se combinan todas las respuestas del agente
7. **Persistencia del Output**: La respuesta final se guarda en la base de datos

### 5. Configuración del Programa Principal

#### 5.1 Program.cs

La configuración principal del programa integra todos los servicios:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddOpenApi();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddSemanticKernel(builder.Configuration);
builder.Services.AddAgents();

var app = builder.Build();

// Inicializar base de datos y configurar pipeline
await app.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Mapear endpoints de API
app.MapAgentEndpoints();

app.Run();
```

### 6. Casos de Uso y Ejemplos Prácticos

#### 6.1 Verificación de Estado de Facturas

**Petición del usuario:**
```
"¿Puedes verificar el estado de pago de la factura INV-202412-0001?"
```

**Proceso interno:**
1. El agente identifica que necesita verificar una factura
2. Automáticamente llama a `VerifyPaymentAsync("INV-202412-0001")`
3. El plugin consulta la base de datos
4. Devuelve información formateada con emojis

**Respuesta del agente:**
```
?? **Factura: INV-202412-0001**
?? Cliente: Juan Pérez
?? Descripción: Servicios legales - Compraventa
?? Monto: $1,500.00
?? Fecha de vencimiento: 15/12/2024
? Estado: Pagada
?? Fecha de pago: 20/12/2024
```

#### 6.2 Creación de Facturas

**Petición del usuario:**
```
"Crea una prefactura para juan.perez@email.com por servicios de registro de marca por $2,000 pesos"
```

**Proceso interno:**
1. El agente extrae: email, descripción, monto
2. Llama a `CreateInvoiceDraftAsync("juan.perez@email.com", "servicios de registro de marca", 2000)`
3. Se crea la factura en la base de datos

#### 6.3 Consultas Complejas

**Petición del usuario:**
```
"¿Qué facturas están vencidas y cuánto dinero representan en total?"
```

**Proceso interno:**
1. El agente llama a `GetUnpaidInvoicesAsync()`
2. Filtra las facturas vencidas
3. Calcula el total automáticamente
4. Presenta la información de manera estructurada

### 7. Testing y Validación

#### 7.1 Archivo de Testing HTTP

El archivo `SemanticKernelLearning04.http` proporciona ejemplos completos de testing:

```http
### 1. Iniciar nueva conversación
POST {{SemanticKernelLearning04_HostAddress}}/api/agent/start
Content-Type: application/json

### 2. Verificar estado de factura
POST {{SemanticKernelLearning04_HostAddress}}/api/agent
Content-Type: application/json

{
    "question": "¿Puedes verificar el estado de pago de la factura INV-202412-0001?",
    "threadId": "{{conversationId}}"
}

### 3. Crear nueva factura
POST {{SemanticKernelLearning04_HostAddress}}/api/agent
Content-Type: application/json

{
    "question": "Crea una prefactura para juan.perez@email.com por servicios de registro de marca por $2,000 pesos",
    "threadId": "{{conversationId}}"
}
```

#### 7.2 Características Destacadas del Sistema

**Function Calling Inteligente:**
- El agente decide automáticamente qué funciones llamar
- No requiere sintaxis específica del usuario
- Maneja parámetros opcionales inteligentemente

**Persistencia Completa:**
- Todas las conversaciones se guardan en SQLite
- El historial se mantiene entre sesiones
- Contexto completo disponible para el agente

**Respuestas Visuales:**
- Uso consistente de emojis para mejor UX
- Formato estructurado y fácil de leer
- Información clara y actionable

---

## ?? Conclusión

Este tutorial demuestra cómo crear un sistema completo de inteligencia artificial conversacional que puede:

### ? Logros Técnicos Alcanzados

1. **Integración Seamless**: Semantic Kernel se integra perfectamente con .NET 9 y Entity Framework Core
2. **Function Calling Automático**: El agente puede ejecutar operaciones de negocio complejas sin intervención manual
3. **Persistencia Inteligente**: El historial de conversaciones permite contexto continuo y experiencias personalizadas
4. **APIs REST Estándar**: Fácil integración con cualquier frontend o sistema externo

### ?? Conceptos Clave Aprendidos

1. **Semantic Kernel Agents**: Los agentes conversacionales pueden manejar lógica de negocio compleja
2. **Plugin Architecture**: Los plugins permiten extender las capacidades del agente de manera modular
3. **Function Calling**: La IA puede invocar funciones automáticamente basándose en el contexto
4. **Dependency Injection**: Integración perfecta con el ecosistema .NET para servicios y persistencia
5. **Conversation Threading**: Mantenimiento de contexto a través de múltiples interacciones

### ?? Próximos Pasos y Extensiones

Este sistema puede expandirse fácilmente para incluir:

- **Múltiples Agentes**: Agentes especializados para diferentes dominios
- **Integración con Sistemas Externos**: APIs de facturación, CRM, sistemas contables
- **Análisis Avanzado**: Reportes automáticos y análisis de tendencias
- **Notificaciones Inteligentes**: Alertas automáticas para facturas vencidas
- **Interfaces de Usuario**: Chatbots web, aplicaciones móviles, integración con Teams

### ?? Valor del Enfoque

La combinación de **Semantic Kernel + Function Calling + Persistencia** crea un sistema que no solo entiende lenguaje natural, sino que puede actuar sobre él de manera inteligente y consistente. Esto abre posibilidades para automatizar procesos de negocio complejos manteniendo la flexibilidad de la interacción humana natural.

Este patrón es aplicable a cualquier dominio empresarial: gestión de inventarios, atención al cliente, análisis financiero, recursos humanos, y mucho más. El código presentado sirve como una base sólida para construir sistemas de IA empresariales robustos y escalables.