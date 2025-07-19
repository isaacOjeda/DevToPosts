using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelLearning04.Data;
using SemanticKernelLearning04.Plugins;
using SemanticKernelLearning04.Services;

namespace SemanticKernelLearning04.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ConversationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ConversationService>();
        services.AddScoped<InvoiceService>();
        
        return services;
    }

    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Azure OpenAI from appsettings
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

    public static IServiceCollection AddAgents(this IServiceCollection services)
    {
        services.AddKeyedTransient<ChatCompletionAgent>("AssistantAgent", (sp, _) =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            var agentKernel = kernel.Clone();

            // Register the InvoicesPlugin with dependency injection
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
}