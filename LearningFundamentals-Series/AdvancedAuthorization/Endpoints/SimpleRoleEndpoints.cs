using AdvancedAuthorization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedAuthorization.Endpoints;

/// <summary>
/// Endpoints para demostrar autorización simple por roles individuales
/// </summary>
public static class SimpleRoleEndpoints
{
    /// <summary>
    /// Registra los endpoints de autorización simple por roles
    /// </summary>
    public static void MapSimpleRoleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/simple-roles")
            .WithTags("Simple Role Authorization")
            .WithOpenApi();

        // Endpoint solo para usuarios básicos
        group.MapGet("/user-only", UserOnlyEndpoint)
            .WithName("UserOnlyEndpoint")
            .WithSummary("Solo para usuarios con rol 'User'")
            .WithDescription("Endpoint accesible únicamente por usuarios con rol User")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireRole("User"));

        // Endpoint solo para managers
        group.MapGet("/manager-only", ManagerOnlyEndpoint)
            .WithName("ManagerOnlyEndpoint")
            .WithSummary("Solo para usuarios con rol 'Manager'")
            .WithDescription("Endpoint accesible únicamente por usuarios con rol Manager")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("Manager"));

        // Endpoint solo para administradores
        group.MapGet("/admin-only", AdminOnlyEndpoint)
            .WithName("AdminOnlyEndpoint")
            .WithSummary("Solo para usuarios con rol 'Admin'")
            .WithDescription("Endpoint accesible únicamente por usuarios con rol Admin")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // Endpoint solo para super administradores
        group.MapGet("/superadmin-only", SuperAdminOnlyEndpoint)
            .WithName("SuperAdminOnlyEndpoint")
            .WithSummary("Solo para usuarios con rol 'SuperAdmin'")
            .WithDescription("Endpoint accesible únicamente por usuarios con rol SuperAdmin")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin"));

        // Endpoint solo para auditores
        group.MapGet("/auditor-only", AuditorOnlyEndpoint)
            .WithName("AuditorOnlyEndpoint")
            .WithSummary("Solo para usuarios con rol 'Auditor'")
            .WithDescription("Endpoint accesible únicamente por usuarios con rol Auditor")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireRole("Auditor"));

        // Endpoint que demuestra la diferencia entre roles
        group.MapGet("/role-comparison", RoleComparisonEndpoint)
            .WithName("RoleComparisonEndpoint")
            .WithSummary("Comparación de roles - Todos los usuarios autenticados")
            .WithDescription("Muestra diferentes respuestas según el rol del usuario")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        // Endpoint de ejemplo con lógica condicional basada en roles
        group.MapGet("/conditional-access", ConditionalAccessEndpoint)
            .WithName("ConditionalAccessEndpoint")
            .WithSummary("Acceso condicional basado en roles")
            .WithDescription("Muestra diferentes niveles de información según el rol")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization();
    }

    /// <summary>
    /// Endpoint accesible solo por usuarios con rol 'User'
    /// </summary>
    private static IResult UserOnlyEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";

        var response = new
        {
            Message = $"¡Hola {username}! Este endpoint es exclusivo para usuarios con rol 'User'.",
            RoleRequired = "User",
            AccessLevel = "Básico",
            AvailableFeatures = new[]
            {
                "Ver perfil personal",
                "Cambiar contraseña",
                "Ver dashboard básico",
                "Acceder a documentación"
            },
            Restrictions = new[]
            {
                "No puede gestionar otros usuarios",
                "No puede acceder a configuraciones administrativas",
                "No puede ver reportes confidenciales"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Acceso autorizado para rol User",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint accesible solo por usuarios con rol 'Manager'
    /// </summary>
    private static IResult ManagerOnlyEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Manager";
        var department = context.User.FindFirst("department")?.Value ?? "General";

        var response = new
        {
            Message = $"Bienvenido {username}, Manager del departamento {department}",
            RoleRequired = "Manager",
            AccessLevel = "Gerencial",
            ManagementCapabilities = new[]
            {
                "Gestionar equipo de trabajo",
                "Aprobar solicitudes de vacaciones",
                "Revisar reportes departamentales",
                "Asignar tareas y proyectos"
            },
            TeamInfo = new
            {
                Department = department,
                EstimatedTeamSize = "5-10 personas",
                ResponsibilityLevel = "Medio-Alto"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Acceso autorizado para rol Manager",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint accesible solo por usuarios con rol 'Admin'
    /// </summary>
    private static IResult AdminOnlyEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Admin";

        var response = new
        {
            Message = $"Panel de administración para {username}",
            RoleRequired = "Admin",
            AccessLevel = "Administrativo",
            AdminCapabilities = new[]
            {
                "Gestionar todos los usuarios del sistema",
                "Configurar parámetros del sistema",
                "Acceder a logs de auditoría",
                "Realizar backups manuales",
                "Gestionar roles y permisos"
            },
            SystemControl = new
            {
                CanModifySystemSettings = true,
                CanDeleteUsers = true,
                CanAccessLogs = true,
                CanManageDatabase = true
            },
            SecurityLevel = "Alto"
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Acceso autorizado para rol Admin",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint accesible solo por usuarios con rol 'SuperAdmin'
    /// </summary>
    private static IResult SuperAdminOnlyEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "SuperAdmin";

        var response = new
        {
            Message = $"Control total del sistema para {username}",
            RoleRequired = "SuperAdmin",
            AccessLevel = "Máximo",
            SuperAdminCapabilities = new[]
            {
                "Acceso completo a toda la plataforma",
                "Modificar cualquier configuración del sistema",
                "Gestionar otros administradores",
                "Activar/desactivar modo mantenimiento",
                "Acceso directo a base de datos",
                "Controlar servicios del sistema"
            },
            CriticalOperations = new
            {
                CanShutdownSystem = true,
                CanModifySecurityPolicies = true,
                CanAccessEncryptionKeys = true,
                CanBypassAllRestrictions = true
            },
            Warning = "Este rol tiene acceso irrestricto. Usar con extrema precaución."
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Acceso autorizado para rol SuperAdmin",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint accesible solo por usuarios con rol 'Auditor'
    /// </summary>
    private static IResult AuditorOnlyEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Auditor";

        var response = new
        {
            Message = $"Panel de auditoría para {username}",
            RoleRequired = "Auditor",
            AccessLevel = "Auditoría",
            AuditCapabilities = new[]
            {
                "Acceso de solo lectura a todos los registros",
                "Generar reportes de auditoría",
                "Revisar logs de actividad del sistema",
                "Monitorear cumplimiento de políticas",
                "Acceder a registros históricos"
            },
            AuditScope = new
            {
                CanViewAllUserActivity = true,
                CanGenerateComplianceReports = true,
                CanAccessSecurityLogs = true,
                CanModifyData = false // Solo lectura
            },
            ComplianceInfo = new
            {
                LastAudit = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd"),
                NextScheduledAudit = DateTime.UtcNow.AddDays(23).ToString("yyyy-MM-dd"),
                ComplianceScore = "94.5%"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Acceso autorizado para rol Auditor",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint que muestra diferentes respuestas según el rol del usuario
    /// </summary>
    private static IResult RoleComparisonEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";
        var roles = context.User.FindAll("role").Select(c => c.Value).ToList();
        var primaryRole = roles.FirstOrDefault() ?? "Sin rol";

        var roleHierarchy = new Dictionary<string, int>
        {
            ["User"] = 1,
            ["Auditor"] = 2,
            ["Manager"] = 3,
            ["Admin"] = 4,
            ["SuperAdmin"] = 5
        };

        var maxLevel = roles.Where(r => roleHierarchy.ContainsKey(r))
                           .Max(r => roleHierarchy.TryGetValue(r, out int level) ? level : 0);

        var response = new
        {
            Username = username,
            AllRoles = roles,
            PrimaryRole = primaryRole,
            AccessLevel = maxLevel,
            Comparison = new
            {
                YourLevel = GetLevelDescription(maxLevel),
                AvailableEndpoints = GetAvailableEndpoints(roles),
                RolePrivileges = GetRolePrivileges(roles),
                SecurityClearance = GetSecurityClearance(maxLevel)
            },
            RoleMatrix = new
            {
                User = new { Level = 1, Description = "Acceso básico al sistema" },
                Auditor = new { Level = 2, Description = "Solo lectura + reportes de auditoría" },
                Manager = new { Level = 3, Description = "Gestión de equipos y aprobaciones" },
                Admin = new { Level = 4, Description = "Administración del sistema" },
                SuperAdmin = new { Level = 5, Description = "Control total del sistema" }
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = $"Comparación de roles para {username}",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint con acceso condicional basado en roles
    /// </summary>
    private static IResult ConditionalAccessEndpoint(HttpContext context)
    {
        var roles = context.User.FindAll("role").Select(c => c.Value).ToList();
        var username = context.User.Identity?.Name ?? "Usuario";
        var department = context.User.FindFirst("department")?.Value;

        // Información básica (todos los usuarios autenticados)
        var basicInfo = new
        {
            ServerTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            SystemStatus = "Operativo",
            YourUsername = username
        };

        // Información adicional según roles
        var conditionalData = new Dictionary<string, object>
        {
            ["BasicInfo"] = basicInfo
        };

        if (roles.Contains("User") || roles.Any())
        {
            conditionalData["UserInfo"] = new
            {
                Message = "Información básica del usuario",
                Department = department ?? "No asignado",
                LastAccess = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        if (roles.Contains("Manager") || roles.Contains("Admin") || roles.Contains("SuperAdmin"))
        {
            conditionalData["ManagementInfo"] = new
            {
                Message = "Información para roles de gestión",
                ActiveUsers = 127,
                SystemLoad = "68%",
                PendingTasks = 15
            };
        }

        if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
        {
            conditionalData["AdminInfo"] = new
            {
                Message = "Información administrativa",
                DatabaseConnections = 45,
                MemoryUsage = "2.1 GB",
                ErrorCount = 3,
                LastBackup = DateTime.UtcNow.AddHours(-4).ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        if (roles.Contains("SuperAdmin"))
        {
            conditionalData["SuperAdminInfo"] = new
            {
                Message = "Información crítica del sistema",
                RootAccess = true,
                EncryptionStatus = "Activo",
                SecurityLevel = "Máximo",
                SystemSecrets = "Accesibles"
            };
        }

        if (roles.Contains("Auditor"))
        {
            conditionalData["AuditInfo"] = new
            {
                Message = "Información de auditoría",
                AuditTrailEntries = 1542,
                ComplianceStatus = "Cumplido",
                LastSecurityScan = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd"),
                VulnerabilitiesFound = 0
            };
        }

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = $"Información condicional para {username} con roles: {string.Join(", ", roles)}",
            Data = conditionalData
        });
    }

    /// <summary>
    /// Obtiene la descripción del nivel de acceso
    /// </summary>
    private static string GetLevelDescription(int level)
    {
        return level switch
        {
            1 => "Básico - Usuario estándar",
            2 => "Auditoría - Solo lectura especializada",
            3 => "Gerencial - Gestión de equipos",
            4 => "Administrativo - Control del sistema",
            5 => "Máximo - Control total",
            _ => "Sin privilegios"
        };
    }

    /// <summary>
    /// Obtiene los endpoints disponibles según los roles
    /// </summary>
    private static string[] GetAvailableEndpoints(List<string> roles)
    {
        var endpoints = new List<string>();

        if (roles.Contains("User"))
            endpoints.Add("/simple-roles/user-only");

        if (roles.Contains("Manager"))
            endpoints.Add("/simple-roles/manager-only");

        if (roles.Contains("Admin"))
            endpoints.Add("/simple-roles/admin-only");

        if (roles.Contains("SuperAdmin"))
            endpoints.Add("/simple-roles/superadmin-only");

        if (roles.Contains("Auditor"))
            endpoints.Add("/simple-roles/auditor-only");

        return endpoints.ToArray();
    }

    /// <summary>
    /// Obtiene los privilegios según los roles
    /// </summary>
    private static string[] GetRolePrivileges(List<string> roles)
    {
        var privileges = new List<string>();

        if (roles.Contains("User"))
            privileges.AddRange(new[] { "Acceso básico", "Ver perfil" });

        if (roles.Contains("Manager"))
            privileges.AddRange(new[] { "Gestión de equipo", "Aprobaciones" });

        if (roles.Contains("Admin"))
            privileges.AddRange(new[] { "Administración", "Configuración del sistema" });

        if (roles.Contains("SuperAdmin"))
            privileges.AddRange(new[] { "Control total", "Modo mantenimiento" });

        if (roles.Contains("Auditor"))
            privileges.AddRange(new[] { "Solo lectura", "Reportes de auditoría" });

        return privileges.ToArray();
    }

    /// <summary>
    /// Obtiene el nivel de seguridad según el nivel de acceso
    /// </summary>
    private static string GetSecurityClearance(int level)
    {
        return level switch
        {
            1 => "Público",
            2 => "Restringido",
            3 => "Confidencial",
            4 => "Secreto",
            5 => "Alto Secreto",
            _ => "Sin clearance"
        };
    }
}
