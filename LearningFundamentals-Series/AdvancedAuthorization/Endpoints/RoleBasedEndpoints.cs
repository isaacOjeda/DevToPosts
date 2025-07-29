using AdvancedAuthorization.Models;
using AdvancedAuthorization.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedAuthorization.Endpoints;

/// <summary>
/// Endpoints para demostrar autorización basada en roles
/// </summary>
public static class RoleBasedEndpoints
{
    /// <summary>
    /// Registra los endpoints de autorización basada en roles
    /// </summary>
    public static void MapRoleBasedEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/roles")
            .WithTags("Role-Based Authorization")
            .WithOpenApi();

        // Endpoints para usuarios básicos
        group.MapGet("/user/profile", GetUserProfile)
            .WithName("GetUserProfile")
            .WithSummary("Obtiene el perfil del usuario - Solo Users y superiores")
            .WithDescription("Endpoint accesible por usuarios con rol User o superior")
            .Produces<ApiResponse<UserInfo>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole("User", "Manager", "Admin", "SuperAdmin", "Auditor"));

        group.MapGet("/user/dashboard", GetUserDashboard)
            .WithName("GetUserDashboard")
            .WithSummary("Dashboard básico del usuario - Solo Users y superiores")
            .WithDescription("Dashboard con información básica para usuarios autenticados")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("User", "Manager", "Admin", "SuperAdmin", "Auditor"));

        // Endpoints para managers
        group.MapGet("/manager/team", GetTeamInfo)
            .WithName("GetTeamInfo")
            .WithSummary("Información del equipo - Solo Managers y superiores")
            .WithDescription("Información del equipo accesible solo para managers y superiores")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("Manager", "Admin", "SuperAdmin"));

        group.MapPost("/manager/approve", ApproveRequest)
            .WithName("ApproveRequest")
            .WithSummary("Aprobar solicitudes - Solo Managers y superiores")
            .WithDescription("Endpoint para aprobar solicitudes del equipo")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("Manager", "Admin", "SuperAdmin"));

        // Endpoints para administradores
        group.MapGet("/admin/system", GetSystemInfo)
            .WithName("GetSystemInfo")
            .WithSummary("Información del sistema - Solo Admins y SuperAdmins")
            .WithDescription("Información administrativa del sistema")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));

        group.MapPost("/admin/settings", UpdateSystemSettings)
            .WithName("UpdateSystemSettings")
            .WithSummary("Actualizar configuración - Solo Admins y SuperAdmins")
            .WithDescription("Endpoint para modificar configuraciones del sistema")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));

        group.MapDelete("/admin/users/{userId}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Eliminar usuario - Solo Admins y SuperAdmins")
            .WithDescription("Eliminar un usuario del sistema")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));

        // Endpoints para super administradores
        group.MapGet("/superadmin/logs", GetSystemLogs)
            .WithName("GetSystemLogs")
            .WithSummary("Logs del sistema - Solo SuperAdmins")
            .WithDescription("Acceso completo a logs del sistema")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin"));

        group.MapPost("/superadmin/maintenance", StartMaintenance)
            .WithName("StartMaintenance")
            .WithSummary("Iniciar mantenimiento - Solo SuperAdmins")
            .WithDescription("Poner el sistema en modo mantenimiento")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin"));

        // Endpoints para auditores
        group.MapGet("/auditor/reports", GetAuditReports)
            .WithName("GetAuditReports")
            .WithSummary("Reportes de auditoría - Solo Auditores y SuperAdmins")
            .WithDescription("Acceso a reportes de auditoría del sistema")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("Auditor", "SuperAdmin"));

        // Endpoint combinado que requiere múltiples roles
        group.MapGet("/combined/management", GetManagementData)
            .WithName("GetManagementData")
            .WithSummary("Datos de gestión - Managers, Admins y SuperAdmins")
            .WithDescription("Información de gestión para roles de liderazgo")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("Manager", "Admin", "SuperAdmin"));
    }

    /// <summary>
    /// Obtiene el perfil del usuario actual
    /// </summary>
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

    /// <summary>
    /// Dashboard básico del usuario
    /// </summary>
    private static IResult GetUserDashboard(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";
        var roles = context.User.FindAll("role").Select(c => c.Value).ToList();

        var dashboardData = new
        {
            WelcomeMessage = $"¡Bienvenido, {username}!",
            UserRoles = roles,
            LastLogin = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            AvailableActions = GetActionsForRoles(roles),
            SystemStatus = "Operativo"
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Dashboard cargado exitosamente",
            Data = dashboardData
        });
    }

    /// <summary>
    /// Información del equipo para managers
    /// </summary>
    private static IResult GetTeamInfo(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Manager";
        var department = context.User.FindFirst("department")?.Value ?? "General";

        var teamData = new
        {
            Manager = username,
            Department = department,
            TeamSize = 8,
            ActiveProjects = 3,
            PendingApprovals = 2,
            TeamMembers = new[]
            {
                new { Name = "Juan Pérez", Role = "Developer", Status = "Activo" },
                new { Name = "María García", Role = "Designer", Status = "Activo" },
                new { Name = "Carlos López", Role = "Analyst", Status = "En licencia" }
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Información del equipo obtenida exitosamente",
            Data = teamData
        });
    }

    /// <summary>
    /// Aprobar solicitudes para managers
    /// </summary>
    private static IResult ApproveRequest([FromBody] object request)
    {
        // Simulación de aprobación
        return Results.Ok(new ApiResponse
        {
            Success = true,
            Message = "Solicitud aprobada exitosamente"
        });
    }

    /// <summary>
    /// Información del sistema para administradores
    /// </summary>
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

    /// <summary>
    /// Actualizar configuraciones del sistema
    /// </summary>
    private static IResult UpdateSystemSettings([FromBody] object settings)
    {
        // Simulación de actualización
        return Results.Ok(new ApiResponse
        {
            Success = true,
            Message = "Configuraciones actualizadas exitosamente"
        });
    }

    /// <summary>
    /// Eliminar usuario para administradores
    /// </summary>
    private static IResult DeleteUser(int userId)
    {
        if (userId <= 0)
        {
            return Results.BadRequest(new ApiResponse
            {
                Success = false,
                Message = "ID de usuario inválido"
            });
        }

        // Simulación de eliminación
        return Results.Ok(new ApiResponse
        {
            Success = true,
            Message = $"Usuario {userId} eliminado exitosamente"
        });
    }

    /// <summary>
    /// Logs del sistema para super administradores
    /// </summary>
    private static IResult GetSystemLogs()
    {
        var logs = new
        {
            TotalEntries = 15420,
            ErrorCount = 12,
            WarningCount = 45,
            RecentLogs = new[]
            {
                new { Level = "INFO", Message = "Usuario admin inició sesión", Timestamp = DateTime.UtcNow.AddMinutes(-5) },
                new { Level = "WARN", Message = "Conexión lenta a base de datos", Timestamp = DateTime.UtcNow.AddMinutes(-15) },
                new { Level = "ERROR", Message = "Fallo en backup automático", Timestamp = DateTime.UtcNow.AddHours(-2) }
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Logs del sistema obtenidos exitosamente",
            Data = logs
        });
    }

    /// <summary>
    /// Iniciar mantenimiento para super administradores
    /// </summary>
    private static IResult StartMaintenance()
    {
        return Results.Ok(new ApiResponse
        {
            Success = true,
            Message = "Modo mantenimiento activado. Sistema en mantenimiento por 30 minutos."
        });
    }

    /// <summary>
    /// Reportes de auditoría para auditores
    /// </summary>
    private static IResult GetAuditReports()
    {
        var auditData = new
        {
            GeneratedAt = DateTime.UtcNow,
            Period = "Últimos 30 días",
            Summary = new
            {
                TotalActions = 2847,
                UserLogins = 1245,
                AdminActions = 89,
                SystemChanges = 23,
                SecurityEvents = 4
            },
            RecentActivity = new[]
            {
                new { User = "admin", Action = "Usuario eliminado", Target = "user_123", Timestamp = DateTime.UtcNow.AddHours(-1) },
                new { User = "manager", Action = "Solicitud aprobada", Target = "request_456", Timestamp = DateTime.UtcNow.AddHours(-3) },
                new { User = "itadmin", Action = "Configuración actualizada", Target = "system_config", Timestamp = DateTime.UtcNow.AddHours(-5) }
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Reportes de auditoría obtenidos exitosamente",
            Data = auditData
        });
    }

    /// <summary>
    /// Datos de gestión combinados
    /// </summary>
    private static IResult GetManagementData(HttpContext context)
    {
        var roles = context.User.FindAll("role").Select(c => c.Value).ToList();
        var department = context.User.FindFirst("department")?.Value ?? "General";

        var managementData = new
        {
            AccessLevel = GetAccessLevel(roles),
            Department = department,
            Metrics = new
            {
                ActiveProjects = 12,
                TeamPerformance = 85.5,
                BudgetUtilization = 72.3,
                CustomerSatisfaction = 4.2
            },
            Alerts = new[]
            {
                new { Type = "Warning", Message = "Presupuesto del proyecto Alpha al 90%", Priority = "Medium" },
                new { Type = "Info", Message = "Nueva actualización del sistema disponible", Priority = "Low" }
            },
            QuickActions = GetQuickActionsForRoles(roles)
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Datos de gestión obtenidos exitosamente",
            Data = managementData
        });
    }

    /// <summary>
    /// Obtiene las acciones disponibles según los roles
    /// </summary>
    private static string[] GetActionsForRoles(List<string> roles)
    {
        var actions = new List<string> { "Ver perfil", "Cambiar contraseña" };

        if (roles.Contains("Manager") || roles.Contains("Admin") || roles.Contains("SuperAdmin"))
            actions.AddRange(new[] { "Gestionar equipo", "Aprobar solicitudes" });

        if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
            actions.AddRange(new[] { "Configurar sistema", "Gestionar usuarios" });

        if (roles.Contains("SuperAdmin"))
            actions.AddRange(new[] { "Acceso completo", "Modo mantenimiento" });

        if (roles.Contains("Auditor"))
            actions.AddRange(new[] { "Ver reportes", "Generar auditorías" });

        return actions.ToArray();
    }

    /// <summary>
    /// Obtiene el nivel de acceso según los roles
    /// </summary>
    private static string GetAccessLevel(List<string> roles)
    {
        if (roles.Contains("SuperAdmin")) return "Completo";
        if (roles.Contains("Admin")) return "Administrativo";
        if (roles.Contains("Manager")) return "Gerencial";
        if (roles.Contains("Auditor")) return "Auditoría";
        return "Básico";
    }

    /// <summary>
    /// Obtiene acciones rápidas según los roles
    /// </summary>
    private static string[] GetQuickActionsForRoles(List<string> roles)
    {
        var actions = new List<string>();

        if (roles.Contains("Manager"))
            actions.AddRange(new[] { "Aprobar solicitudes pendientes", "Ver reportes de equipo" });

        if (roles.Contains("Admin"))
            actions.AddRange(new[] { "Backup del sistema", "Revisar logs" });

        if (roles.Contains("SuperAdmin"))
            actions.AddRange(new[] { "Configuración avanzada", "Monitoreo en tiempo real" });

        return actions.ToArray();
    }
}
