using DecoratorPattern.Interfaces;
using DecoratorPattern.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// Registrar servicios decorados y sus dependencias
builder.Services.AddKeyedScoped<IWeatherService, WeatherService>("WeatherService");
builder.Services.AddKeyedScoped<IWeatherService, CachedWeatherService>("CachedWeatherService");
builder.Services.AddScoped<IWeatherService, LogWeatherService>();

// builder.Services.AddScoped<IWeatherService>(sp =>
//     new LogWeatherService(
//         new CachedWeatherService(
//             new WeatherService(sp.GetRequiredService<IHttpClientFactory>()),
//             sp.GetRequiredService<IMemoryCache>()),
//         sp.GetRequiredService<ILogger<LogWeatherService>>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Definir una ruta para obtener el pronóstico del clima a través del servicio decorado
app.MapGet("api/weather", (IWeatherService weatherService, double lon, double lat) =>
    weatherService.GetWeatherForecastAsync(lat, lon));

app.Run();