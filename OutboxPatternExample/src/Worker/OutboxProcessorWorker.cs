using System.Text.Json;
using ApplicationCore.Common.Events;
using ApplicationCore.Entities;
using ApplicationCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Worker;

public class OutboxProcessorWorker : BackgroundService
{
    private readonly ILogger<OutboxProcessorWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public OutboxProcessorWorker(ILogger<OutboxProcessorWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Consulta los outbox messages
            var outboxMessages = await dbContext.OutboxMessages
                .Where(x => x.Finished == false)
                .ToListAsync(stoppingToken);

            _logger.LogInformation("Found {outboxMessages.Count} messages to send", outboxMessages.Count);

            foreach (var outboxMessage in outboxMessages)
            {
                await HandleOutboxMessage(outboxMessage, dbContext);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task HandleOutboxMessage(OutboxMessage outboxMessage, AppDbContext dbContext)
    {
        switch (outboxMessage.Type)
        {
            case OutboxMessageType.PaymentCreated:
                await HandlePaymentCreated(outboxMessage, dbContext);
                break;
            default:
                _logger.LogWarning("Unknown message type {outboxMessage.Type}", outboxMessage.Type);
                break;
        }
    }

    private async Task HandlePaymentCreated(OutboxMessage outboxMessage, AppDbContext dbContext)
    {
        var paymentCreatedEvent = JsonSerializer.Deserialize<PaymentCreatedEvent>(outboxMessage.EventBody);

        if (paymentCreatedEvent == null)
        {
            _logger.LogWarning("PaymentCreatedEvent is null");
            return;
        }

        var payment = await dbContext.Payments.FindAsync(paymentCreatedEvent.PaymentId);

        if (payment == null)
        {
            _logger.LogWarning("Payment {paymentCreatedEvent.PaymentId} not found", paymentCreatedEvent.PaymentId);
            return;
        }

        _logger.LogInformation("Payment {paymentCreatedEvent.PaymentId} found", paymentCreatedEvent.PaymentId);
        _logger.LogInformation("Simulating payment processing...");

        // Simulate Payment with random time
        await Task.Delay(new Random().Next(1000, 5000));

        // Randomly reject payments for testing purposes
        if (new Random().Next(0, 10) < 3)
        {
            _logger.LogError("Payment {paymentCreatedEvent.PaymentId} rejected", paymentCreatedEvent.PaymentId);
            outboxMessage.FailedAt = DateTime.Now;
            outboxMessage.FailureReason = "Payment rejected";
            outboxMessage.Retries++;

            if (outboxMessage.Retries > 3)
            {
                outboxMessage.Finished = true;
                outboxMessage.FailureReason = "Max retries reached";
            }

            await dbContext.SaveChangesAsync();

            return;
        }


        payment.Status = PaymentStatus.Paid;

        outboxMessage.ProcessedAt = DateTime.Now;
        outboxMessage.Finished = true;

        outboxMessage.FailedAt = null;
        outboxMessage.FailureReason = null;

        await dbContext.SaveChangesAsync();
    }
}
