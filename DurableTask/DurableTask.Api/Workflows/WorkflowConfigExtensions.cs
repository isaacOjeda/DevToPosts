using DurableTask.Api.Workflows.CreatePayment;
using DurableTask.AzureStorage;
using DurableTask.Core;

namespace DurableTask.Api.Workflows;

internal static class WorkflowConfigExtensions
{

    public static WebApplicationBuilder AddWorkflows(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<PaymentOrchestrator>();
        builder.Services.AddTransient<CreatePaymentActivity>();
        builder.Services.AddTransient<CreateInvoiceActivity>();
        // Azure Storage Accounts configuration
        builder.Services.AddTransient(_ => new AzureStorageOrchestrationServiceSettings
        {
            StorageConnectionString = builder.Configuration["DurableTask:StorageConnectionString"],
            TaskHubName = builder.Configuration["DurableTask:HubName"]
        });
        // Task Hub Client
        builder.Services.AddTransient(sc =>
        {

            var serviceSettings = sc.GetRequiredService<AzureStorageOrchestrationServiceSettings>();
            var azureStorageOrchestrationService = new AzureStorageOrchestrationService(serviceSettings);

            return new TaskHubClient(azureStorageOrchestrationService);
        });


        builder.Services.AddHostedService<WorkflowWorker>();


        return builder;
    }
}
