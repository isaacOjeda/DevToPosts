using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Text.Json;
using WebApi;
using WebApi.Models;

var builder = WebApplication.CreateBuilder(args);

const string outputTemplate =
    "[{Level:w}]: {Timestamp:dd-MM-yyyy:HH:mm:ss} {MachineName} {EnvironmentName} {SourceContext} {Message}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: outputTemplate)
    .WriteTo.OpenTelemetry(opts =>
    {
        opts.ResourceAttributes = new Dictionary<string, object>
        {
            ["app"] = "webapi",
            ["runtime"] = "dotnet",
            ["service.name"] = "WebApi"
        };
    })
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddSingleton<Instrumentor>();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(Instrumentor.ServiceName)
        .ConfigureResource(resource => resource
            .AddService(Instrumentor.ServiceName))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation(opts =>
        {
            opts.FilterHttpRequestMessage = req =>
            {
                var ignore = new[] { "/loki/api" };
                return !ignore.Any(s => req.RequestUri!.ToString().Contains(s));
            };
        })
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddMeter(Instrumentor.ServiceName)
        .AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel")
        .ConfigureResource(resource => resource
            .AddService(Instrumentor.ServiceName))
        .AddRuntimeInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddProcessInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEventCountersInstrumentation(c =>
            {
                // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/available-counters
                c.AddEventSources(
                    "Microsoft.AspNetCore.Hosting",
                    "Microsoft-AspNetCore-Server-Kestrel",
                    "System.Net.Http",
                    "System.Net.Sockets");
            })
        .AddOtlpExporter());


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.MapGet("/api/weather-forecast", async (HttpClient http, Instrumentor instrumentor) =>
{
    instrumentor.IncomingRequestCounter.Add(1,
        new KeyValuePair<string, object?>("operation", "GetWeatherForecast"),
        new KeyValuePair<string, object?>("minimal-api-route", "/api/weather-forecast"));

    var url = "https://api.open-meteo.com/v1/forecast?latitude=28.68&longitude=-106.04&hourly=temperature_2m";

    var response = await http.GetAsync(url);

    response.EnsureSuccessStatusCode();

    var rawData = await response.Content.ReadAsStringAsync();


    var data = JsonSerializer.Deserialize<WeatherForecast>(rawData);

    return data;
});

app.MapGet("error-example", () =>
{
    throw new Exception("Error example");
});


app.Run();
