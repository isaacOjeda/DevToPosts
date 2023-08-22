using HealthChecks.ApplicationStatus.DependencyInjection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Web;

var builder = WebApplication.CreateBuilder(args);

// Health checks
builder.Services
    .AddHealthChecks()
    .AddApplicationStatus(name: "api_status", tags: new[] { "api" })
    .AddSqlServer(     
        connectionString: builder.Configuration.GetConnectionString("Default")!,
        name: "sql",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "db", "sql", "sqlserver" })
    .AddSendGrid(apiKey: builder.Configuration["SendGrid:Key"]!, tags: new[] { "email", "sendgrid" })
    .AddCheck<ServerHealthCheck>("server_health_check", tags: new []{"custom", "api"});

// Health UI
builder.Services
    .AddHealthChecksUI()
    .AddInMemoryStorage();


var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapHealthChecks("/healthz", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI();

app.Run();
