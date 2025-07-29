using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AdvancedAuthorization.Services;
using AdvancedAuthorization.Endpoints;
using AdvancedAuthorization.Authorization.Handlers;
using AdvancedAuthorization.Authorization.Services;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Register custom services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPolicyBuilder, PolicyBuilder>();

// Register custom authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MultiplePermissionsAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AnyPermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, RoleWithPermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, WorkingHoursAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, DepartmentAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ConditionalAccessAuthorizationHandler>();

// Configure Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ??
                    throw new InvalidOperationException("JWT Key not found")))
        };
    });

// Configure Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuthEndpoints();
app.MapRoleBasedEndpoints();
app.MapSimpleRoleEndpoints();
app.MapClaimsBasedEndpoints();
app.MapCustomAuthorizationEndpoints();

// Default endpoint for testing
app.MapGet("/", () => new
{
    Message = "Advanced Authorization API",
    Version = "1.0.0",
    Endpoints = new
    {
        // Authentication
        Login = "/auth/login",
        Me = "/auth/me",
        TestUsers = "/auth/test-users",
        Refresh = "/auth/refresh",

        // Role-based Authorization
        RoleBased = "/role-based/**",
        SimpleRoles = "/simple-roles/**",

        // Claims-based Authorization
        ClaimsBased = "/claims-based/**",

        // Custom Authorization (Requirements & Handlers)
        CustomAuth = "/custom-auth/**",

        // Documentation
        OpenAPI = "/openapi/v1.json"
    }
})
.WithName("Root")
.WithSummary("API Root")
.AllowAnonymous();

app.Run();

