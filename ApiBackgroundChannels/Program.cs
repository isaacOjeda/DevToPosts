using System.Threading.Channels;
using ApiBackgroundChannels.Endpoints;
using ApiBackgroundChannels.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar el Channel como Singleton con opciones optimizadas
// Bounded Channel con backpressure para prevenir sobrecarga del sistema
builder.Services.AddSingleton(Channel.CreateBounded<JobCommand>(
    new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.Wait, // Espera cuando está lleno (backpressure)
        SingleWriter = false, // Múltiples endpoints pueden escribir
        SingleReader = true   // Solo un BackgroundService consume
    }));

// Registrar el Background Service
builder.Services.AddHostedService<JobProcessor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapJobEndpoints();

app.Run();