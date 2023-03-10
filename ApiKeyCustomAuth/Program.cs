using ApiKeyCustomAuth.Auth;
using ApiKeyCustomAuth.Data;
using ApiKeyCustomAuth.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApiDbContext>(options => 
    options.UseInMemoryDatabase(nameof(ApiDbContext)));

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(ApiKeySchemeOptions.Scheme)
    .AddScheme<ApiKeySchemeOptions, ApiKeySchemeHandler>(
        ApiKeySchemeOptions.Scheme, options => 
        {
            options.HeaderName = "X-API-KEY";
        });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (HttpRequest request) => 
{
    return new 
    {
        request.HttpContext.User.Identity.Name,
        Claims = request.HttpContext.User.Claims
            .Select(s => new 
            {
                s.Type,
                s.Value
            })
    };
}).RequireAuthorization();

await Seed();

app.Run();



async Task Seed()
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetService<ApiDbContext>();

    if (!await context.ApiKeys.AnyAsync())
    {
        context.ApiKeys.Add(new ApiKey
        {
            Key = Guid.Parse("0e6b2066-9e98-4783-8c82-c3530aa8a197"),
            Name = "App 1"
        });

        context.ApiKeys.Add(new ApiKey
        {
            Key = Guid.Parse("607de3e9-2d01-430d-a6e1-d2ff8b6cfcf0"),
            Name = "App 2"
        });

        await context.SaveChangesAsync();
    }
}