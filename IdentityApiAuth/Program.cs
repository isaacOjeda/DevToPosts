using IdentityApiAuth.Data;
using IdentityApiAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Authentication & Authorization
builder.Services
    .AddAuthorization()
    .AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);

// Identity Core
builder.Services
    .AddIdentityCore<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<User>();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/me", (HttpContext httpContext) =>
{
    return new
    {
        httpContext.User.Identity.Name,
        httpContext.User.Identity.AuthenticationType,
        Claims = httpContext.User.Claims.Select(s => new
        {
            s.Type, s.Value
        }).ToList()
    };
}).RequireAuthorization();

app.Run();