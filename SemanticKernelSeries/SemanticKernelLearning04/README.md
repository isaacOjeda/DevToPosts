# Sistema de Gesti�n de Facturas con Semantic Kernel y SQLite

Este proyecto demuestra c�mo crear un sistema completo de gesti�n de facturas usando **Semantic Kernel**, **Entity Framework Core** y **SQLite** para persistir datos.

## ?? Caracter�sticas

### ?? Gesti�n de Facturas
- ? Verificar estado de pago de facturas espec�ficas
- ?? Crear prefacturas (borradores) para clientes
- ?? Consultar facturas pendientes de pago
- ?? Marcar facturas como pagadas
- ?? Gesti�n de informaci�n de clientes
- ?? Identificaci�n autom�tica de facturas vencidas

### ?? Inteligencia Artificial
- **Semantic Kernel Agents** para procesamiento de lenguaje natural
- **Function Calling** autom�tico para ejecutar acciones espec�ficas
- **Persistencia de conversaciones** en SQLite
- **Respuestas contextuales** con historial de conversaci�n

## ??? Arquitectura

```
SemanticKernelLearning04/
??? Models/
?   ??? Conversation.cs          # Modelo para conversaciones
?   ??? ConversationMessage.cs   # Mensajes de conversaci�n
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
- **InvoiceNumber**: N�mero �nico de factura
- **Customer**: Cliente asociado
- **Description**: Descripci�n del servicio
- **Amount**: Monto de la factura
- **Status**: Estado (Borrador, Enviada, Pagada, Vencida, Cancelada)
- **DueDate**: Fecha de vencimiento
- **PaidDate**: Fecha de pago (si aplica)

### Customer (Cliente)
- **Name**: Nombre completo
- **Email**: Email �nico
- **Phone**: Tel�fono
- **Address**: Direcci�n

## ??? Configuraci�n

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
- **Datos de muestra** se crean autom�ticamente al iniciar

### 3. Configuraci�n del Agent
```csharp
var agent = new ChatCompletionAgent()
{
    Name = "Asistente",
    Instructions = """
                   Eres un asistente de abogados en una notar�a p�blica. 
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
Verifica el estado de una factura espec�fica.
```
"�Puedes verificar el estado de pago de la factura INV-202412-0001?"
```

### 2. CreateInvoiceDraftAsync
Crea una nueva prefactura para un cliente.
```
"Crea una prefactura para juan.perez@email.com por servicios de registro de marca por $2,000 pesos"
```

### 3. GetUnpaidInvoicesAsync
Obtiene todas las facturas sin pagar.
```
"Mu�strame todas las facturas sin pagar"
```

### 4. MarkInvoiceAsPaidAsync
Marca una factura como pagada.
```
"Marca la factura INV-202412-0002 como pagada"
```

### 5. GetCustomerInfoAsync
Obtiene informaci�n detallada de un cliente.
```
"Dame informaci�n detallada del cliente maria.garcia@email.com"
```

## ?? Datos de Muestra

El sistema incluye datos de muestra que se crean autom�ticamente:

### Clientes
- Juan P�rez (juan.perez@email.com)
- Mar�a Garc�a (maria.garcia@email.com)
- Carlos L�pez (carlos.lopez@email.com)
- Ana Rodr�guez (ana.rodriguez@email.com)

### Facturas
- **INV-202412-0001**: Servicios legales - Compraventa (PAGADA)
- **INV-202412-0002**: Constituci�n de sociedad (ENVIADA)
- **INV-202412-0003**: Testamento p�blico abierto (VENCIDA)
- **INV-202412-0004**: Poder notarial (BORRADOR)
- **INV-202412-0005**: Cancelaci�n de hipoteca (VENCIDA)

## ?? Pruebas

Utiliza el archivo `SemanticKernelLearning04.http` para probar todas las funcionalidades:

1. **Iniciar conversaci�n**: Crear un nuevo hilo de conversaci�n
2. **Verificar facturas**: Consultar estado de facturas espec�ficas
3. **Gestionar pagos**: Marcar facturas como pagadas
4. **Crear facturas**: Generar nuevas prefacturas
5. **Consultas complejas**: Usar lenguaje natural para consultas avanzadas

## ?? Caracter�sticas Destacadas

### Respuestas Visuales
Las respuestas incluyen emojis y formato para mejor legibilidad:
```
?? **Factura: INV-202412-0001**
?? Cliente: Juan P�rez
?? Descripci�n: Servicios legales - Compraventa
?? Monto: $1,500.00
?? Fecha de vencimiento: 15/12/2024
? Estado: Pagada
?? Fecha de pago: 20/12/2024
```

### Function Calling Inteligente
El agente selecciona autom�ticamente la funci�n correcta bas�ndose en la consulta del usuario, sin necesidad de especificar par�metros t�cnicos.

### Persistencia Completa
- Conversaciones guardadas en SQLite
- Historial completo de interacciones
- Datos de facturas persistentes entre sesiones

## ?? Conceptos Aprendidos

1. **Semantic Kernel Agents**: Configuraci�n y uso de agentes inteligentes
2. **Function Calling**: Creaci�n de plugins con funciones espec�ficas
3. **Entity Framework Core**: Mapeo de objetos y relaciones
4. **SQLite Integration**: Persistencia de datos local
5. **Dependency Injection**: Inyecci�n de servicios en plugins
6. **Async/Await Patterns**: Manejo as�ncrono de datos
7. **Business Logic**: Implementaci�n de reglas de negocio

Este ejemplo es perfecto para demostrar c�mo combinar IA conversacional con persistencia de datos real en aplicaciones empresariales.

---

# Tutorial: Sistema de Gesti�n de Facturas con Semantic Kernel y Agentes de IA

## ?? Introducci�n

Este tutorial te guiar� paso a paso para crear un sistema completo de gesti�n de facturas utilizando **Microsoft Semantic Kernel** y **agentes conversacionales**. El proyecto demuestra c�mo integrar inteligencia artificial con APIs REST para crear un asistente que puede manejar operaciones de negocio complejas usando lenguaje natural.

### �Qu� aprender�s?

- **Configuraci�n de Semantic Kernel**: Integraci�n con Azure OpenAI
- **Creaci�n de Agentes Conversacionales**: ChatCompletionAgent para procesamiento de lenguaje natural
- **Function Calling**: Plugins personalizados que el agente puede ejecutar autom�ticamente
- **API Endpoints**: Exposici�n de funcionalidades a trav�s de REST APIs
- **Persistencia de Conversaciones**: Almacenamiento del historial en SQLite

### �Qu� construiremos?

Un sistema completo donde los usuarios pueden:
- Verificar estados de facturas usando lenguaje natural
- Crear nuevas facturas con comandos simples
- Consultar informaci�n de clientes
- Gestionar pagos y estados de facturas
- Mantener conversaciones contextuales con historial persistente

---

## ?? Desarrollo

### 1. Configuraci�n Inicial del Proyecto

#### 1.1 Dependencias Necesarias

Primero, necesitamos instalar los paquetes NuGet requeridos para Semantic Kernel y Entity Framework:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
<PackageReference Include="Microsoft.SemanticKernel" Version="1.60.0" />
<PackageReference Include="Microsoft.SemanticKernel.Agents.Core" Version="1.60.0" />
```

#### 1.2 Configuraci�n de Azure OpenAI

En `appsettings.json`, configura tu instancia de Azure OpenAI:

```json
{
  "AzureOpenAI": {
    "DeploymentName": "gpt-4",
    "ApiKey": "tu-api-key-aqu�",
    "Endpoint": "https://tu-instancia.openai.azure.com/",
    "ModelId": "gpt-4"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=conversations.db"
  }
}
```

### 2. Creaci�n del Plugin de Facturas

#### 2.1 Estructura del Plugin

Un plugin en Semantic Kernel es una clase que contiene m�todos decorados con `[KernelFunction]`. Estos m�todos pueden ser invocados autom�ticamente por el agente:

```csharp
public class InvoicesPlugin
{
    private readonly InvoiceService _invoiceService;

    public InvoicesPlugin(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [KernelFunction]
    [Description("Verifica el estado de pago de una factura espec�fica usando su n�mero de factura.")]
    public async Task<string> VerifyPaymentAsync(
        [Description("N�mero de la factura a verificar (ej: INV-202412-0001)")] 
        string numeroFactura)
    {
        // Implementaci�n del m�todo
    }
}
```

#### 2.2 Funciones Clave del Plugin

**Verificaci�n de Pagos:**
```csharp
[KernelFunction]
[Description("Verifica el estado de pago de una factura espec�fica usando su n�mero de factura.")]
public async Task<string> VerifyPaymentAsync([Description("N�mero de la factura a verificar")] string numeroFactura)
{
    try
    {
        var invoice = await _invoiceService.GetInvoiceByNumberAsync(numeroFactura);
        
        if (invoice == null)
        {
            return $"? No se encontr� ninguna factura con el n�mero: {numeroFactura}";
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

**Creaci�n de Facturas:**
```csharp
[KernelFunction]
[Description("Realiza una prefactura (borrador) para un cliente espec�fico.")]
public async Task<string> CreateInvoiceDraftAsync(
    [Description("Email del cliente para la prefactura")] string clienteEmail,
    [Description("Descripci�n del servicio o trabajo realizado")] string descripcion,
    [Description("Monto de la factura en pesos")] decimal monto,
    [Description("D�as hasta el vencimiento (opcional, por defecto 30 d�as)")] int diasVencimiento = 30)
{
    // Implementaci�n completa en el archivo del proyecto
}
```

### 3. Configuraci�n del Agente Conversacional

#### 3.1 Registro de Servicios

Utilizamos la inyecci�n de dependencias para configurar todos los servicios necesarios:

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

#### 3.2 Creaci�n del Agente

El agente se configura como un servicio con clave para poder inyectarlo espec�ficamente:

```csharp
public static IServiceCollection AddAgents(this IServiceCollection services)
{
    services.AddKeyedTransient<ChatCompletionAgent>("AssistantAgent", (sp, _) =>
    {
        var kernel = sp.GetRequiredService<Kernel>();
        var agentKernel = kernel.Clone();

        // Registrar el InvoicesPlugin con inyecci�n de dependencias
        var invoiceService = sp.GetRequiredService<InvoiceService>();
        agentKernel.ImportPluginFromObject(new InvoicesPlugin(invoiceService));

        var agent = new ChatCompletionAgent()
        {
            Name = "Asistente",
            Instructions = """
                       Eres un asistente de abogados en una notar�a p�blica. 
                       Tu tarea es ayudar a los abogados a gestionar y verificar el estado de las facturas.
                       
                       Funciones disponibles:
                       - Verificar el estado de pago de facturas espec�ficas
                       - Crear prefacturas para clientes
                       - Consultar facturas pendientes de pago
                       - Marcar facturas como pagadas
                       - Consultar informaci�n de clientes
                       
                       Siempre proporciona informaci�n clara y detallada sobre el estado de las facturas.
                       Usa emojis para hacer las respuestas m�s visuales y f�ciles de entender.
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

**Puntos clave de la configuraci�n:**

1. **Instructions**: Define el comportamiento y personalidad del agente
2. **FunctionChoiceBehavior.Auto()**: Permite al agente decidir autom�ticamente qu� funciones llamar
3. **Kernel Clone**: Cada agente tiene su propia instancia del kernel con sus plugins espec�ficos

### 4. Implementaci�n de Endpoints REST

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

        // Endpoint para iniciar conversaci�n
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
            // Implementaci�n del endpoint principal
        });

        return app;
    }
}
```

#### 4.2 Endpoint Principal de Conversaci�n

Este es el coraz�n del sistema, donde se maneja la interacci�n con el agente:

```csharp
group.MapPost("/", async (AskQuestionRequest request,
    [FromKeyedServices("AssistantAgent")] ChatCompletionAgent agent,
    ConversationService conversationService) =>
{
    try
    {
        // 1. Obtener historial de conversaci�n desde la base de datos
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

        // 5. Crear hilo de conversaci�n con el historial
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

1. **Recuperaci�n del Contexto**: Se obtiene el historial completo de la conversaci�n
2. **Persistencia del Input**: El mensaje del usuario se guarda inmediatamente
3. **Construcci�n del Contexto**: Se recrea el historial para el agente
4. **Invocaci�n del Agente**: El agente procesa el mensaje con todo el contexto
5. **Function Calling**: El agente puede llamar m�ltiples funciones si es necesario
6. **Agregaci�n de Respuestas**: Se combinan todas las respuestas del agente
7. **Persistencia del Output**: La respuesta final se guarda en la base de datos

### 5. Configuraci�n del Programa Principal

#### 5.1 Program.cs

La configuraci�n principal del programa integra todos los servicios:

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

### 6. Casos de Uso y Ejemplos Pr�cticos

#### 6.1 Verificaci�n de Estado de Facturas

**Petici�n del usuario:**
```
"�Puedes verificar el estado de pago de la factura INV-202412-0001?"
```

**Proceso interno:**
1. El agente identifica que necesita verificar una factura
2. Autom�ticamente llama a `VerifyPaymentAsync("INV-202412-0001")`
3. El plugin consulta la base de datos
4. Devuelve informaci�n formateada con emojis

**Respuesta del agente:**
```
?? **Factura: INV-202412-0001**
?? Cliente: Juan P�rez
?? Descripci�n: Servicios legales - Compraventa
?? Monto: $1,500.00
?? Fecha de vencimiento: 15/12/2024
? Estado: Pagada
?? Fecha de pago: 20/12/2024
```

#### 6.2 Creaci�n de Facturas

**Petici�n del usuario:**
```
"Crea una prefactura para juan.perez@email.com por servicios de registro de marca por $2,000 pesos"
```

**Proceso interno:**
1. El agente extrae: email, descripci�n, monto
2. Llama a `CreateInvoiceDraftAsync("juan.perez@email.com", "servicios de registro de marca", 2000)`
3. Se crea la factura en la base de datos

#### 6.3 Consultas Complejas

**Petici�n del usuario:**
```
"�Qu� facturas est�n vencidas y cu�nto dinero representan en total?"
```

**Proceso interno:**
1. El agente llama a `GetUnpaidInvoicesAsync()`
2. Filtra las facturas vencidas
3. Calcula el total autom�ticamente
4. Presenta la informaci�n de manera estructurada

### 7. Testing y Validaci�n

#### 7.1 Archivo de Testing HTTP

El archivo `SemanticKernelLearning04.http` proporciona ejemplos completos de testing:

```http
### 1. Iniciar nueva conversaci�n
POST {{SemanticKernelLearning04_HostAddress}}/api/agent/start
Content-Type: application/json

### 2. Verificar estado de factura
POST {{SemanticKernelLearning04_HostAddress}}/api/agent
Content-Type: application/json

{
    "question": "�Puedes verificar el estado de pago de la factura INV-202412-0001?",
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

#### 7.2 Caracter�sticas Destacadas del Sistema

**Function Calling Inteligente:**
- El agente decide autom�ticamente qu� funciones llamar
- No requiere sintaxis espec�fica del usuario
- Maneja par�metros opcionales inteligentemente

**Persistencia Completa:**
- Todas las conversaciones se guardan en SQLite
- El historial se mantiene entre sesiones
- Contexto completo disponible para el agente

**Respuestas Visuales:**
- Uso consistente de emojis para mejor UX
- Formato estructurado y f�cil de leer
- Informaci�n clara y actionable

---

## ?? Conclusi�n

Este tutorial demuestra c�mo crear un sistema completo de inteligencia artificial conversacional que puede:

### ? Logros T�cnicos Alcanzados

1. **Integraci�n Seamless**: Semantic Kernel se integra perfectamente con .NET 9 y Entity Framework Core
2. **Function Calling Autom�tico**: El agente puede ejecutar operaciones de negocio complejas sin intervenci�n manual
3. **Persistencia Inteligente**: El historial de conversaciones permite contexto continuo y experiencias personalizadas
4. **APIs REST Est�ndar**: F�cil integraci�n con cualquier frontend o sistema externo

### ?? Conceptos Clave Aprendidos

1. **Semantic Kernel Agents**: Los agentes conversacionales pueden manejar l�gica de negocio compleja
2. **Plugin Architecture**: Los plugins permiten extender las capacidades del agente de manera modular
3. **Function Calling**: La IA puede invocar funciones autom�ticamente bas�ndose en el contexto
4. **Dependency Injection**: Integraci�n perfecta con el ecosistema .NET para servicios y persistencia
5. **Conversation Threading**: Mantenimiento de contexto a trav�s de m�ltiples interacciones

### ?? Pr�ximos Pasos y Extensiones

Este sistema puede expandirse f�cilmente para incluir:

- **M�ltiples Agentes**: Agentes especializados para diferentes dominios
- **Integraci�n con Sistemas Externos**: APIs de facturaci�n, CRM, sistemas contables
- **An�lisis Avanzado**: Reportes autom�ticos y an�lisis de tendencias
- **Notificaciones Inteligentes**: Alertas autom�ticas para facturas vencidas
- **Interfaces de Usuario**: Chatbots web, aplicaciones m�viles, integraci�n con Teams

### ?? Valor del Enfoque

La combinaci�n de **Semantic Kernel + Function Calling + Persistencia** crea un sistema que no solo entiende lenguaje natural, sino que puede actuar sobre �l de manera inteligente y consistente. Esto abre posibilidades para automatizar procesos de negocio complejos manteniendo la flexibilidad de la interacci�n humana natural.

Este patr�n es aplicable a cualquier dominio empresarial: gesti�n de inventarios, atenci�n al cliente, an�lisis financiero, recursos humanos, y mucho m�s. El c�digo presentado sirve como una base s�lida para construir sistemas de IA empresariales robustos y escalables.