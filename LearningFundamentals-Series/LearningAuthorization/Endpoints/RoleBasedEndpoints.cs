using AdvancedAuthorization.Models;
using AdvancedAuthorization.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningAuthorization.Endpoints;


public static class RoleBasedEndpoints
{
    public static void MapRoleBasedEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/role-based")
            .WithTags("Role-Based Authorization")
            .WithOpenApi();


        group.MapGet("/user/profile", GetUserProfile)
            .WithName("GetUserProfile")
            .WithSummary("Obtiene el perfil del usuario - Solo Users y superiores")
            .WithDescription("Endpoint accesible por usuarios con rol User o superior")
            .Produces<ApiResponse<UserInfo>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole("User", "Manager", "Admin", "SuperAdmin", "Auditor"));

        // Endpoint que requiere el rol de administrador (Admin)
        group.MapGet("/admin/system", GetSystemInfo)
            .WithName("GetSystemInfo")
            .WithSummary("Información del sistema - Solo Admins y SuperAdmins")
            .WithDescription("Información administrativa del sistema")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));

    }

    private static async Task<IResult> GetUserProfile(
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
                Message = "Perfil obtenido exitosamente",
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

    private static IResult GetSystemInfo()
    {
        var systemData = new
        {
            SystemVersion = "2.1.0",
            Environment = "Production",
            Uptime = "15 días, 4 horas",
            ActiveUsers = 127,
            DatabaseStatus = "Operativo",
            MemoryUsage = "68%",
            DiskSpace = "42% utilizado",
            LastBackup = DateTime.UtcNow.AddHours(-6).ToString("yyyy-MM-dd HH:mm:ss")
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Información del sistema obtenida exitosamente",
            Data = systemData
        });
    }

}