using System.Text.Json;
using ApplicationCore;
using ApplicationCore.Common.Events;
using ApplicationCore.Common.Models;
using ApplicationCore.Entities;
using ApplicationCore.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationCore();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/api/payment", async (PaymentRequest request, AppDbContext context) =>
{
    var payment = new Payment
    {
        PaymentId = Guid.NewGuid(),
        TokenCard = request.TokenCard,
        Amount = request.Amount,
        Currency = request.Currency,
        CardHolderEmail = request.CardHolderEmail,
        CardHolderName = request.CardHolderName,
        Status = PaymentStatus.Pending
    };

    context.Payments.Add(payment);
    context.OutboxMessages.Add(new OutboxMessage
    {
        EventType = "PaymentCreatedEvent",
        EventBody = JsonSerializer.Serialize(new PaymentCreatedEvent
        (
            payment.PaymentId,
            payment.TokenCard,
            payment.Amount,
            payment.Currency,
            payment.CardHolderEmail,
            payment.CardHolderName
        )),
    });

    await context.SaveChangesAsync();

    return Results.Ok(new
    {
        Message = "Payment request created successfully",
        payment.PaymentId        
    });
});

app.MapGet("/api/payment/status/{paymentId}", async (Guid paymentId, AppDbContext context) =>
{
    var payment = await context.Payments.FindAsync(paymentId);

    if (payment is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new 
    {
        payment.PaymentId,
        Status = payment.Status.ToString()
    });
});

app.Run();




