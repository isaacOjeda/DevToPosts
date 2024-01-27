using DurableTask.Core;
using DurableTask.Core.Exceptions;

namespace DurableTask.Api.Workflows.CreatePayment;

public class PaymentOrchestrator(ILogger<PaymentOrchestrator> logger)
    : TaskOrchestration<PaymentResponse, CreatePaymentRequest>
{
    public override async Task<PaymentResponse> RunTask(OrchestrationContext context, CreatePaymentRequest input)
    {
        logger.LogInformation(
            $"\nIs Replaying = {context.IsReplaying} InstanceId = {context.OrchestrationInstance.InstanceId} Execution ID = {context.OrchestrationInstance.ExecutionId}\n");

        logger.LogInformation("Running Orchestration");

        var paymentResponse = await CreatePayment(context, input);
        var invoiceResponse = await CreateInvoice(context, input, paymentResponse);

        logger.LogInformation("Orchestration completed");

        return new PaymentResponse(paymentResponse.PaymentId, invoiceResponse.InvoiceId);
    }

    private async Task<CreateInvoiceResponse?> CreateInvoice(OrchestrationContext context, CreatePaymentRequest input,
        CreatePaymentResponse paymentResponse)
    {
        CreateInvoiceResponse? invoiceResponse;

        try
        {
            var retryOptions = new RetryOptions(TimeSpan.FromSeconds(10), 5);

            invoiceResponse = await context.ScheduleWithRetry<CreateInvoiceResponse>(typeof(CreateInvoiceActivity),
                retryOptions, new CreateInvoiceRequest(input.OrderId, paymentResponse.PaymentId));
        }
        catch (TaskFailedException ex)
        {
            logger.LogError(ex, "Failed to create invoice");

            return null;
        }

        return invoiceResponse;
    }

    private async Task<CreatePaymentResponse> CreatePayment(OrchestrationContext context, CreatePaymentRequest input)
    {
        return await context.ScheduleTask<CreatePaymentResponse>(typeof(CreatePaymentActivity), input);
    }
}


public record PaymentResponse(string PaymentId, string InvoiceId);