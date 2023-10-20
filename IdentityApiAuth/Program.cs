using IdentityApiAuth.Data;
using IdentityApiAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services
    .AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorizationBuilder();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services
    .AddIdentityCore<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapIdentityApi<User>();

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