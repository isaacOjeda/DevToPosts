using AdvancedAuthorization.Models;
using AdvancedAuthorization.Models.Constants;

namespace LearningAuthorization.Endpoints;

public static class ClaimsBasedEndpoints
{
    public static void MapClaimsBasedEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/claims-based")
            .WithTags("Claims-Based Authorization")
            .WithOpenApi();


        // Endpoint que requiere el departamento de recursos humanos (HR)
        group.MapGet("/users/read", UsersReadEndpoint)
            .WithName("UsersReadEndpoint")
            .WithSummary("Requiere claim 'permission:users.read'")
            .WithDescription("Solo usuarios con permiso de lectura de usuarios pueden acceder")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireClaim(AuthConstants.ClaimTypes.Department, "HR"));
    }


    private static IResult UsersReadEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var response = new
        {
            Message = $"Acceso autorizado para {username} - Lectura de usuarios",
            RequiredClaim = "department:hr",
            Operation = "Leer lista de usuarios del sistema",
            UserPermissions = userPermissions,
            Data = new
            {
                Users = new[]
                {
                    new { Id = 1, Name = "Juan Pérez", Role = "User", Status = "Activo" },
                    new { Id = 2, Name = "María García", Role = "Manager", Status = "Activo" },
                    new { Id = 3, Name = "Carlos López", Role = "Admin", Status = "Inactivo" },
                    new { Id = 4, Name = "Ana Martínez", Role = "User", Status = "Activo" }
                },
                TotalUsers = 4,
                ActiveUsers = 3,
                LastUpdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Acceso autorizado con claim users.read",
            Data = response
        });
    }
}