using ApplicationCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationCore();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
