using AdvancedAuthorization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdvancedAuthorization.Endpoints;

/// <summary>
/// Endpoints para demostrar autorización basada en claims específicos
/// </summary>
public static class ClaimsBasedEndpoints
{
    /// <summary>
    /// Registra los endpoints de autorización basada en claims
    /// </summary>
    public static void MapClaimsBasedEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/claims-based")
            .WithTags("Claims-Based Authorization")
            .WithOpenApi();

        // Endpoint que requiere un claim específico de lectura de usuarios
        group.MapGet("/users/read", UsersReadEndpoint)
            .WithName("UsersReadEndpoint")
            .WithSummary("Requiere claim 'permission:users.read'")
            .WithDescription("Solo usuarios con permiso de lectura de usuarios pueden acceder")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequireClaim("permission", "users.read"));

        // Endpoint que requiere claim de escritura de usuarios
        group.MapPost("/users/create", UsersCreateEndpoint)
            .WithName("UsersCreateEndpoint")
            .WithSummary("Requiere claim 'permission:users.write'")
            .WithDescription("Solo usuarios con permiso de escritura de usuarios pueden crear")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireClaim("permission", "users.write"));

        // Endpoint que requiere claim de eliminación de usuarios
        group.MapDelete("/users/delete", UsersDeleteEndpoint)
            .WithName("UsersDeleteEndpoint")
            .WithSummary("Requiere claim 'permission:users.delete'")
            .WithDescription("Solo usuarios con permiso de eliminación pueden acceder")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireClaim("permission", "users.delete"));

        // Endpoint que requiere claims de reportes
        group.MapGet("/reports/generate", ReportsGenerateEndpoint)
            .WithName("ReportsGenerateEndpoint")
            .WithSummary("Requiere claim 'permission:reports.read'")
            .WithDescription("Solo usuarios con permiso de lectura de reportes pueden generar")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireClaim("permission", "reports.read"));

        // Endpoint que requiere claims administrativos
        group.MapGet("/admin/sensitive-data", AdminSensitiveDataEndpoint)
            .WithName("AdminSensitiveDataEndpoint")
            .WithSummary("Requiere claim 'permission:admin.access'")
            .WithDescription("Solo usuarios con acceso administrativo pueden ver datos sensibles")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy => policy.RequireClaim("permission", "admin.access"));

        // Endpoint que requiere múltiples claims (AND lógico)
        group.MapPost("/admin/critical-operation", CriticalOperationEndpoint)
            .WithName("CriticalOperationEndpoint")
            .WithSummary("Requiere múltiples claims: admin.access Y system.write")
            .WithDescription("Operación crítica que requiere múltiples permisos")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy =>
            {
                policy.RequireClaim("permission", "admin.access");
                policy.RequireClaim("permission", "system.write");
            });

        // Endpoint que acepta varios claims (OR lógico)
        group.MapGet("/data/read-any", DataReadAnyEndpoint)
            .WithName("DataReadAnyEndpoint")
            .WithSummary("Acepta cualquiera: users.read O reports.read O admin.access")
            .WithDescription("Acceso si tienes al menos uno de varios permisos")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy =>
                policy.RequireClaim("permission", "users.read", "reports.read", "admin.access"));

        // Endpoint que analiza todos los claims del usuario
        group.MapGet("/claims/analysis", ClaimsAnalysisEndpoint)
            .WithName("ClaimsAnalysisEndpoint")
            .WithSummary("Análisis completo de claims del usuario")
            .WithDescription("Muestra todos los claims y permisos del usuario actual")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        // Endpoint con validación condicional de claims
        group.MapGet("/conditional-claims", ConditionalClaimsEndpoint)
            .WithName("ConditionalClaimsEndpoint")
            .WithSummary("Validación condicional de claims en tiempo de ejecución")
            .WithDescription("Diferentes respuestas según los claims del usuario")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        // Endpoint que combina roles y claims
        group.MapGet("/hybrid/role-and-claims", HybridAuthorizationEndpoint)
            .WithName("HybridAuthorizationEndpoint")
            .WithSummary("Autorización híbrida: rol Manager + claim users.read")
            .WithDescription("Demuestra combinación de roles y claims")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization(policy =>
            {
                policy.RequireRole("Manager", "Admin", "SuperAdmin");
                policy.RequireClaim("permission", "users.read");
            });
    }

    /// <summary>
    /// Endpoint que requiere permiso de lectura de usuarios
    /// </summary>
    private static IResult UsersReadEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var response = new
        {
            Message = $"Acceso autorizado para {username} - Lectura de usuarios",
            RequiredClaim = "permission:users.read",
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

    /// <summary>
    /// Endpoint que requiere permiso de escritura de usuarios
    /// </summary>
    private static IResult UsersCreateEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var response = new
        {
            Message = $"Operación autorizada para {username} - Crear usuario",
            RequiredClaim = "permission:users.write",
            Operation = "Crear nuevo usuario en el sistema",
            UserPermissions = userPermissions,
            ActionDetails = new
            {
                CanCreateUsers = true,
                CanAssignRoles = userPermissions.Contains("admin.access"),
                CanSetPermissions = userPermissions.Contains("admin.access"),
                MaxUsersCanCreate = userPermissions.Contains("admin.access") ? "Ilimitado" : "10 por mes"
            },
            SimulatedCreation = new
            {
                NewUserId = new Random().Next(1000, 9999),
                Status = "Usuario creado exitosamente",
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Usuario creado con autorización users.write",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint que requiere permiso de eliminación de usuarios
    /// </summary>
    private static IResult UsersDeleteEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var response = new
        {
            Message = $"Operación peligrosa autorizada para {username} - Eliminar usuario",
            RequiredClaim = "permission:users.delete",
            Operation = "Eliminar usuario del sistema",
            UserPermissions = userPermissions,
            SecurityWarning = "Esta es una operación irreversible",
            ActionDetails = new
            {
                CanDeleteUsers = true,
                RequiresConfirmation = true,
                AuditLogEntry = $"Usuario {username} autorizó eliminación",
                BackupRequired = true
            },
            SimulatedDeletion = new
            {
                DeletedUserId = 1001,
                Status = "Usuario eliminado exitosamente",
                BackupCreated = true,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Usuario eliminado con autorización users.delete",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint que requiere permiso de lectura de reportes
    /// </summary>
    private static IResult ReportsGenerateEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var response = new
        {
            Message = $"Generación de reportes autorizada para {username}",
            RequiredClaim = "permission:reports.read",
            Operation = "Generar reportes del sistema",
            UserPermissions = userPermissions,
            AvailableReports = new[]
            {
                new { Name = "Reporte de Usuarios", Type = "UserReport", Enabled = true },
                new { Name = "Reporte de Actividad", Type = "ActivityReport", Enabled = true },
                new { Name = "Reporte Financiero", Type = "FinancialReport", Enabled = userPermissions.Contains("admin.access") },
                new { Name = "Reporte de Auditoría", Type = "AuditReport", Enabled = userPermissions.Contains("admin.access") }
            },
            GeneratedReport = new
            {
                ReportId = Guid.NewGuid().ToString(),
                Type = "UserActivityReport",
                GeneratedBy = username,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                RecordCount = 156,
                Status = "Completado"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Reporte generado con autorización reports.read",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint que requiere acceso administrativo
    /// </summary>
    private static IResult AdminSensitiveDataEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Administrador";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var response = new
        {
            Message = $"Acceso a datos sensibles autorizado para {username}",
            RequiredClaim = "permission:admin.access",
            SecurityLevel = "ALTO",
            UserPermissions = userPermissions,
            SensitiveData = new
            {
                SystemKeys = new[]
                {
                    "JWT_SECRET_KEY: ********",
                    "DATABASE_CONNECTION: Server=****;Database=****",
                    "API_KEY_EXTERNAL: ********"
                },
                SystemHealth = new
                {
                    CPUUsage = "45%",
                    MemoryUsage = "2.1GB / 8GB",
                    DiskSpace = "150GB / 500GB",
                    ActiveConnections = 23,
                    ErrorCount = 2
                },
                SecurityMetrics = new
                {
                    FailedLoginAttempts = 5,
                    ActiveSessions = 12,
                    LastSecurityScan = DateTime.UtcNow.AddHours(-6).ToString("yyyy-MM-dd HH:mm"),
                    VulnerabilitiesFound = 0
                }
            },
            AccessLog = new
            {
                AccessedBy = username,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                IPAddress = "192.168.1.100",
                UserAgent = "API-Client"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Datos sensibles accedidos con autorización admin.access",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint que requiere múltiples claims (AND)
    /// </summary>
    private static IResult CriticalOperationEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Administrador";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var response = new
        {
            Message = $"Operación crítica autorizada para {username}",
            RequiredClaims = new[] { "permission:admin.access", "permission:system.write" },
            SecurityLevel = "CRÍTICO",
            UserPermissions = userPermissions,
            CriticalOperation = new
            {
                Type = "SystemMaintenanceMode",
                Description = "Activar modo de mantenimiento del sistema",
                Impact = "Todos los usuarios serán desconectados temporalmente",
                EstimatedDowntime = "15 minutos",
                BackupStatus = "Completo",
                RollbackPlan = "Disponible"
            },
            ExecutionDetails = new
            {
                ScheduledBy = username,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                ApprovalRequired = false, // Ya validado por claims
                AuditTrail = $"Operación crítica autorizada para {username} con permisos múltiples"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Operación crítica ejecutada con autorización múltiple",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint que acepta cualquiera de varios claims (OR)
    /// </summary>
    private static IResult DataReadAnyEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var acceptedPermissions = new[] { "users.read", "reports.read", "admin.access" };
        var matchingPermissions = userPermissions.Intersect(acceptedPermissions).ToList();

        var response = new
        {
            Message = $"Acceso a datos autorizado para {username}",
            AcceptedClaims = acceptedPermissions,
            UserMatchingClaims = matchingPermissions,
            AccessLevel = DetermineAccessLevel(matchingPermissions),
            UserPermissions = userPermissions,
            AvailableData = GetAvailableDataByPermissions(matchingPermissions),
            AccessReason = $"Usuario tiene {string.Join(", ", matchingPermissions)}"
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Acceso autorizado con al menos uno de varios claims",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint que analiza todos los claims del usuario
    /// </summary>
    private static IResult ClaimsAnalysisEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";
        var allClaims = context.User.Claims.ToList();

        var claimsByType = allClaims.GroupBy(c => c.Type)
                                  .ToDictionary(g => g.Key, g => g.Select(c => c.Value).ToArray());

        var response = new
        {
            Message = $"Análisis completo de claims para {username}",
            TotalClaims = allClaims.Count,
            ClaimsByType = claimsByType,
            PermissionClaims = allClaims.Where(c => c.Type == "permission").Select(c => c.Value).ToList(),
            RoleClaims = allClaims.Where(c => c.Type == "role").Select(c => c.Value).ToList(),
            StandardClaims = new
            {
                Name = context.User.FindFirst(ClaimTypes.Name)?.Value,
                NameIdentifier = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Role = context.User.FindFirst(ClaimTypes.Role)?.Value,
                Email = context.User.FindFirst(ClaimTypes.Email)?.Value
            },
            CustomClaims = allClaims.Where(c => !IsStandardClaim(c.Type))
                                  .Select(c => new { Type = c.Type, Value = c.Value })
                                  .ToList(),
            AuthorizationCapabilities = AnalyzeAuthorizationCapabilities(allClaims)
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Análisis de claims completado",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint con validación condicional de claims
    /// </summary>
    private static IResult ConditionalClaimsEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var conditionalData = new Dictionary<string, object>
        {
            ["BaseInfo"] = new
            {
                Username = username,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                AccessLevel = "Básico"
            }
        };

        // Datos condicionales basados en claims específicos
        if (userPermissions.Contains("users.read"))
        {
            conditionalData["UserData"] = new
            {
                Message = "Acceso a datos de usuarios habilitado",
                UserCount = 127,
                ActiveUsers = 89
            };
        }

        if (userPermissions.Contains("reports.read"))
        {
            conditionalData["ReportData"] = new
            {
                Message = "Acceso a reportes habilitado",
                AvailableReports = 15,
                LastGenerated = DateTime.UtcNow.AddHours(-2).ToString("yyyy-MM-dd HH:mm")
            };
        }

        if (userPermissions.Contains("admin.access"))
        {
            conditionalData["AdminData"] = new
            {
                Message = "Acceso administrativo habilitado",
                SystemStatus = "Operativo",
                MaintenanceMode = false,
                CriticalAlerts = 0
            };
        }

        if (userPermissions.Contains("users.write"))
        {
            conditionalData["WriteOperations"] = new
            {
                Message = "Operaciones de escritura habilitadas",
                CanCreateUsers = true,
                CanModifyUsers = true,
                DailyQuota = "50 operaciones restantes"
            };
        }

        if (userPermissions.Contains("users.delete"))
        {
            conditionalData["DangerousOperations"] = new
            {
                Message = "Operaciones peligrosas habilitadas",
                CanDeleteUsers = true,
                RequiresApproval = false,
                Warning = "Operaciones irreversibles disponibles"
            };
        }

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = $"Datos condicionales para {username} basados en claims",
            Data = conditionalData
        });
    }

    /// <summary>
    /// Endpoint que combina roles y claims (autorización híbrida)
    /// </summary>
    private static IResult HybridAuthorizationEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";
        var userRoles = context.User.FindAll("role").Select(c => c.Value).ToList();
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var response = new
        {
            Message = $"Autorización híbrida exitosa para {username}",
            RequiredRole = "Manager, Admin, o SuperAdmin",
            RequiredClaim = "permission:users.read",
            UserRoles = userRoles,
            UserPermissions = userPermissions,
            HybridCapabilities = new
            {
                ManagementAccess = userRoles.Intersect(new[] { "Manager", "Admin", "SuperAdmin" }).Any(),
                UserReadAccess = userPermissions.Contains("users.read"),
                CombinedAuthorization = "Autorización exitosa con ambos requisitos"
            },
            AdvancedFeatures = new
            {
                TeamManagement = userRoles.Contains("Manager"),
                SystemAdministration = userRoles.Contains("Admin"),
                FullSystemControl = userRoles.Contains("SuperAdmin"),
                UserDataAccess = userPermissions.Contains("users.read"),
                UserDataModification = userPermissions.Contains("users.write"),
                UserDataDeletion = userPermissions.Contains("users.delete")
            },
            RecommendedActions = GetRecommendedActions(userRoles, userPermissions)
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Autorización híbrida (rol + claim) completada",
            Data = response
        });
    }

    #region Helper Methods

    /// <summary>
    /// Determina el nivel de acceso basado en los permisos
    /// </summary>
    private static string DetermineAccessLevel(List<string> permissions)
    {
        if (permissions.Contains("admin.access")) return "Administrativo";
        if (permissions.Contains("reports.read")) return "Reportes";
        if (permissions.Contains("users.read")) return "Usuarios";
        return "Básico";
    }

    /// <summary>
    /// Obtiene los datos disponibles según los permisos
    /// </summary>
    private static object GetAvailableDataByPermissions(List<string> permissions)
    {
        var data = new Dictionary<string, object>();

        if (permissions.Contains("users.read"))
        {
            data["UserData"] = new { TotalUsers = 127, ActiveUsers = 89 };
        }

        if (permissions.Contains("reports.read"))
        {
            data["ReportData"] = new { AvailableReports = 15, LastUpdate = DateTime.UtcNow.AddHours(-1) };
        }

        if (permissions.Contains("admin.access"))
        {
            data["AdminData"] = new { SystemHealth = "OK", ActiveSessions = 23 };
        }

        return data;
    }

    /// <summary>
    /// Verifica si un tipo de claim es estándar
    /// </summary>
    private static bool IsStandardClaim(string claimType)
    {
        var standardTypes = new[]
        {
            ClaimTypes.Name,
            ClaimTypes.NameIdentifier,
            ClaimTypes.Role,
            ClaimTypes.Email,
            "exp", "iat", "nbf", "iss", "aud"
        };
        return standardTypes.Contains(claimType);
    }

    /// <summary>
    /// Analiza las capacidades de autorización basadas en claims
    /// </summary>
    private static object AnalyzeAuthorizationCapabilities(List<Claim> claims)
    {
        var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
        var roles = claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();

        return new
        {
            CanReadUsers = permissions.Contains("users.read"),
            CanWriteUsers = permissions.Contains("users.write"),
            CanDeleteUsers = permissions.Contains("users.delete"),
            CanReadReports = permissions.Contains("reports.read"),
            CanWriteReports = permissions.Contains("reports.write"),
            CanDeleteReports = permissions.Contains("reports.delete"),
            HasAdminAccess = permissions.Contains("admin.access"),
            CanWriteSystem = permissions.Contains("system.write"),
            HasManagementRole = roles.Intersect(new[] { "Manager", "Admin", "SuperAdmin" }).Any(),
            SecurityClearance = GetSecurityClearance(roles, permissions)
        };
    }

    /// <summary>
    /// Obtiene el nivel de seguridad basado en roles y permisos
    /// </summary>
    private static string GetSecurityClearance(List<string> roles, List<string> permissions)
    {
        if (roles.Contains("SuperAdmin")) return "Máximo";
        if (roles.Contains("Admin") || permissions.Contains("admin.access")) return "Alto";
        if (roles.Contains("Manager")) return "Medio";
        if (permissions.Any(p => p.Contains("write"))) return "Medio-Bajo";
        return "Básico";
    }

    /// <summary>
    /// Obtiene acciones recomendadas basadas en roles y permisos
    /// </summary>
    private static string[] GetRecommendedActions(List<string> roles, List<string> permissions)
    {
        var actions = new List<string>();

        if (roles.Contains("Manager") && permissions.Contains("users.read"))
        {
            actions.Add("Revisar equipo de trabajo");
            actions.Add("Generar reportes de actividad");
        }

        if (roles.Contains("Admin") && permissions.Contains("users.read"))
        {
            actions.Add("Gestionar usuarios del sistema");
            actions.Add("Revisar configuraciones de seguridad");
        }

        if (permissions.Contains("users.write"))
        {
            actions.Add("Crear nuevos usuarios");
        }

        if (permissions.Contains("reports.read"))
        {
            actions.Add("Generar reportes personalizados");
        }

        return actions.ToArray();
    }

    #endregion
}
