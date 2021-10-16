using Carter;
using MinimalApis.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwagger();
builder.Services.AddPersistence();
builder.Services.AddCarter();

var app = builder.Build();


app.MapSwagger();
app.MapCarter();

app.Run();
