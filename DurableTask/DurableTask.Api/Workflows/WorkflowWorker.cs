using DurableTask.Api.Workflows.CreatePayment;
using DurableTask.AzureStorage;
using DurableTask.Core;

namespace DurableTask.Api.Workflows;

public class WorkflowWorker(IServiceProvider serviceProvider) : IHostedService
{
    private TaskHubWorker _taskHubWorker = null!;


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = serviceProvider.GetRequiredService<AzureStorageOrchestrationServiceSettings>();
        var azureStorageOrchestrationService = new AzureStorageOrchestrationService(settings);


        _taskHubWorker = new TaskHubWorker(azureStorageOrchestrationService);


        await _taskHubWorker
            .AddTaskOrchestrations(new ServiceProviderObjectCreator<TaskOrchestration>(typeof(PaymentOrchestrator), serviceProvider))
            .AddTaskActivities(new ServiceProviderObjectCreator<TaskActivity>(typeof(CreatePaymentActivity), serviceProvider))
            .AddTaskActivities(new ServiceProviderObjectCreator<TaskActivity>(typeof(CreateInvoiceActivity), serviceProvider))
            .StartAsync();

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _taskHubWorker.StopAsync(true);
    }
}
