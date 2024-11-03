using TestContainers.Web.Data;
using TestContainers.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlServer<AppDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddScoped<AppDbContextSeed>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await SeedDatabase();
}

app.MapProductsEndpoints();

app.Run();

async Task SeedDatabase()
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var seeder = services.GetRequiredService<AppDbContextSeed>();

    await seeder.SeedAsync();
}

public partial class Program {}