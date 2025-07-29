using AdvancedAuthorization.Authorization.Services;
using AdvancedAuthorization.Models;
using LearningAuthorization.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;

namespace LearningAuthorization.Endpoints;

public static class CustomAuthorizationEndpoints
{
    public static void MapCustomAuthorizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/custom-auth")
            .WithTags("Custom Authorization (Requirements & Handlers)")
            .WithOpenApi();


        // Endpoint con validación de permiso personalizado
        group.MapGet("/permission/{permission}", CustomPermissionEndpoint)
            .WithName("CustomPermissionEndpoint")
            .WithSummary("Validación personalizada de permiso específico")
            .WithDescription("Demuestra custom PermissionRequirement y PermissionAuthorizationHandler")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden);

        // AdminAccess permission
        group.MapGet("/admin-access", AdminAccessEndpoint)
            .WithName("AdminAccess")
            .WithSummary("Requires AdminAccess permission")
            .RequireAuthorization(policy =>
            {
                policy.RequirePermissions("admin.access");
            });

        // Endpoint con validación de departamento
        group.MapGet("/department-access", DepartmentAccessEndpoint)
            .WithName("DepartmentAccessEndpoint")
            .WithSummary("Solo para departamentos específicos")
            .WithDescription("Demuestra DepartmentRequirement (IT, HR, Finance)")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);
    }


    private static async Task<IResult> CustomPermissionEndpoint(
        string permission,
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        // Crear política dinámicamente basada en el parámetro
        var policy = PolicyBuilderExtensions.PermissionPolicy(permission);

        // Evaluar la política
        var authResult = await authorizationService.AuthorizeAsync(context.User, policy);

        if (!authResult.Succeeded)
        {
            var failures = authResult.Failure?.FailureReasons?.Select(r => r.Message) ?? new[] { "Acceso denegado" };
            return Results.Json(new ApiResponse
            {
                Success = false,
                Message = $"No tienes el permiso '{permission}' requerido",
                Errors = failures.ToList()
            }, statusCode: StatusCodes.Status403Forbidden);
        }

        var username = context.User.Identity?.Name ?? "Usuario";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var response = new
        {
            Message = $"Acceso autorizado para {username}",
            ValidatedPermission = permission,
            CustomHandler = "PermissionAuthorizationHandler",
            UserPermissions = userPermissions,
            ValidationDetails = new
            {
                RequirementType = "PermissionRequirement",
                HandlerType = "PermissionAuthorizationHandler",
                PermissionValidated = permission,
                ValidationSuccess = true,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Autorización personalizada exitosa",
            Data = response
        });
    }

    private static IResult AdminAccessEndpoint()
    {
        return Results.Ok(new ApiResponse
        {
            Success = true,
            Message = "Acceso administrativo concedido"
        });
    }


    private static async Task<IResult> DepartmentAccessEndpoint(
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        var policy = PolicyBuilderExtensions.NewPolicy()
            .RequireDepartment("IT", "HR", "Finance")
            .Build();

        var authResult = await authorizationService.AuthorizeAsync(context.User, policy);

        if (!authResult.Succeeded)
        {
            var failures = authResult.Failure?.FailureReasons?.Select(r => r.Message) ?? new[] { "Departamento no autorizado" };
            return Results.Json(new ApiResponse
            {
                Success = false,
                Message = "Solo usuarios de IT, HR o Finance pueden acceder",
                Errors = failures.ToList()
            }, statusCode: StatusCodes.Status403Forbidden);
        }

        var username = context.User.Identity?.Name ?? "Usuario";
        var userDepartment = context.User.FindFirst("department")?.Value ?? "No asignado";

        var response = new
        {
            Message = $"Acceso departamental autorizado para {username}",
            UserDepartment = userDepartment,
            AllowedDepartments = new[] { "IT", "HR", "Finance" },
            CustomHandler = "DepartmentAuthorizationHandler",
            DepartmentAccess = new
            {
                AccessReason = $"Usuario pertenece al departamento: {userDepartment}",
                SecurityModel = "Segmentación por departamento",
                Scalability = "Fácil agregar/quitar departamentos autorizados",
                UseCase = "Acceso basado en estructura organizacional"
            },
            AvailableOperations = GetDepartmentOperations(userDepartment)
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Validación departamental exitosa",
            Data = response
        });
    }


    private static object GetDepartmentOperations(string department)
    {
        return department.ToUpper() switch
        {
            "IT" => new
            {
                PrimaryOperations = new[] { "Gestión de sistemas", "Soporte técnico", "Administración de servidores" },
                SecondaryOperations = new[] { "Gestión de usuarios", "Backup y recuperación", "Monitoreo de red" }
            },
            "HR" => new
            {
                PrimaryOperations = new[] { "Gestión de empleados", "Reclutamiento", "Evaluaciones de desempeño" },
                SecondaryOperations = new[] { "Capacitación", "Políticas de empresa", "Relaciones laborales" }
            },
            "FINANCE" => new
            {
                PrimaryOperations = new[] { "Contabilidad", "Presupuestos", "Reportes financieros" },
                SecondaryOperations = new[] { "Auditoría", "Análisis de costos", "Planificación financiera" }
            },
            _ => new
            {
                PrimaryOperations = new[] { "Operaciones básicas" },
                SecondaryOperations = new[] { "Acceso limitado" }
            }
        };
    }
}