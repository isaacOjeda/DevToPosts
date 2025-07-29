using AdvancedAuthorization.Models;
using AdvancedAuthorization.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningAuthorization.Endpoints;

/// <summary>
/// Endpoints de autenticación del sistema
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Registra los endpoints de autenticación
    /// </summary>
    public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Autentica un usuario y devuelve un token JWT")
            .WithDescription("Endpoint para autenticar usuarios del sistema usando credenciales")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        group.MapGet("/me", GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .WithSummary("Obtiene información del usuario autenticado")
            .WithDescription("Devuelve la información del usuario basada en el token JWT")
            .Produces<ApiResponse<UserInfo>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet("/test-users", GetTestUsers)
            .WithName("GetTestUsers")
            .WithSummary("Obtiene información de usuarios de prueba")
            .WithDescription("Endpoint para obtener credenciales y información de usuarios de prueba")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .AllowAnonymous();
    }

    /// <summary>
    /// Endpoint de login
    /// </summary>
    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        [FromServices] IUserService userService,
        [FromServices] ITokenService tokenService)
    {
        try
        {
            // Validar entrada
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Username y password son requeridos"
                });
            }

            // Autenticar usuario
            var user = await userService.AuthenticateAsync(request.Username, request.Password);
            if (user == null)
            {
                return Results.Json(new ApiResponse
                {
                    Success = false,
                    Message = "Credenciales inválidas o usuario inactivo"
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            // Generar token
            var token = tokenService.GenerateToken(user);
            var expiryMinutes = tokenService.GetTokenExpiryMinutes();

            var response = new AuthResponse
            {
                Token = token,
                ExpiresIn = expiryMinutes * 60, // Convertir a segundos
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    Roles = user.Roles.Select(r => r.ToString()).ToList(),
                    Permissions = user.GetEffectivePermissions().Select(p => p.ToClaimValue()).ToList(),
                    Department = user.Department
                }
            };

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error interno del servidor",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Obtiene información del usuario autenticado
    /// </summary>
    private static async Task<IResult> GetCurrentUserAsync(
        HttpContext context,
        [FromServices] IUserService userService)
    {
        try
        {
            var userIdClaim = context.User.FindFirst("user_id")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Results.Json(new ApiResponse
                {
                    Success = false,
                    Message = "Token inválido"
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var user = await userService.GetByIdAsync(userId);
            if (user == null)
            {
                return Results.NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Usuario no encontrado"
                });
            }

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Roles = user.Roles.Select(r => r.ToString()).ToList(),
                Permissions = user.GetEffectivePermissions().Select(p => p.ToClaimValue()).ToList(),
                Department = user.Department
            };

            return Results.Ok(new ApiResponse<UserInfo>
            {
                Success = true,
                Message = "Usuario obtenido exitosamente",
                Data = userInfo
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error interno del servidor",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Obtiene información de usuarios de prueba
    /// </summary>
    private static IResult GetTestUsers()
    {
        var testCredentials = UserService.GetTestCredentials();
        var testUsersInfo = UserService.GetTestUsersInfo();

        var response = new
        {
            Message = "Usuarios de prueba disponibles. En producción, este endpoint NO debe existir.",
            Credentials = testCredentials.Select(kvp => new
            {
                Username = kvp.Key,
                Password = kvp.Value
            }).ToList(),
            UsersInfo = testUsersInfo.Select(info => new
            {
                Username = info.Username,
                Roles = info.Role,
                Permissions = info.Permissions
            }).ToList()
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Información de usuarios de prueba",
            Data = response
        });
    }
}
