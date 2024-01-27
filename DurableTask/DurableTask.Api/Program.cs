using DurableTask.Api.Workflows;
using DurableTask.Api.Workflows.CreatePayment;
using DurableTask.Core;

var builder = WebApplication.CreateBuilder(args);

builder.AddWorkflows();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/api/payments", async (CreatePaymentRequest request, TaskHubClient client) =>
{
    var instanceId = await client.CreateOrchestrationInstanceAsync(typeof(PaymentOrchestrator), request);

    return Results.Ok(new
    {
        instanceId
    });
});

app.Run();

