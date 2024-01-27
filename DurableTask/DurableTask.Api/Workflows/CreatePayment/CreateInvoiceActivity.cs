using DurableTask.Core;

namespace DurableTask.Api.Workflows.CreatePayment;

public class CreateInvoiceActivity(ILogger<CreateInvoiceActivity> logger)
    : AsyncTaskActivity<CreateInvoiceRequest, CreateInvoiceResponse>
{
    protected override Task<CreateInvoiceResponse> ExecuteAsync(TaskContext context, CreateInvoiceRequest input)
    {

        // Simular una falla aleatoria
        if (new Random().Next(0, 10) > 5)
        {
            logger.LogError("Failed to create invoice");

            throw new Exception("Failed to create invoice");
        }


        logger.LogInformation("\nCreating invoice for order {OrderId} with payment {PaymentId}\n",
                       input.OrderId, input.PaymentId);

        return Task.FromResult(new CreateInvoiceResponse(Guid.NewGuid().ToString()));
    }
}

public record CreateInvoiceRequest(string OrderId, string PaymentId);
public record CreateInvoiceResponse(string InvoiceId);