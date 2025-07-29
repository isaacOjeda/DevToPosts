using System.Text;
using AdvancedAuthorization.Authorization.Handlers;
using AdvancedAuthorization.Authorization.Services;
using AdvancedAuthorization.Services;
using LearningAuthorization.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPolicyBuilder, PolicyBuilder>();

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MultiplePermissionsAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AnyPermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, RoleWithPermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, WorkingHoursAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, DepartmentAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ConditionalAccessAuthorizationHandler>();

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

builder.Services.AddAuthorization();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapClaimsBasedEndpoints();
app.MapRoleBasedEndpoints();

app.Run();

