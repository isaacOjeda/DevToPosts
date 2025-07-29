using AdvancedAuthorization.Models;
using AdvancedAuthorization.Authorization.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedAuthorization.Endpoints;

/// <summary>
/// Endpoints para demostrar autorización con custom requirements y handlers
/// </summary>
public static class CustomAuthorizationEndpoints
{
    /// <summary>
    /// Registra los endpoints de autorización personalizada
    /// </summary>
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

        // Endpoint con validación de múltiples permisos
        group.MapPost("/multiple-permissions", MultiplePermissionsEndpoint)
            .WithName("MultiplePermissionsEndpoint")
            .WithSummary("Requiere múltiples permisos simultáneamente")
            .WithDescription("Demuestra MultiplePermissionsRequirement (adminaccess Y systemconfigwrite)")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);

        // Endpoint con validación de permisos alternativos
        group.MapGet("/any-permission", AnyPermissionEndpoint)
            .WithName("AnyPermissionEndpoint")
            .WithSummary("Acepta cualquiera de varios permisos")
            .WithDescription("Demuestra AnyPermissionRequirement (usersread O reportsread O adminaccess)")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);

        // Endpoint con validación híbrida de roles y permisos
        group.MapGet("/hybrid-validation", HybridValidationEndpoint)
            .WithName("HybridValidationEndpoint")
            .WithSummary("Combina roles y permisos con custom handler")
            .WithDescription("Demuestra RoleWithPermissionRequirement")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);

        // Endpoint con validación de horario laboral
        group.MapGet("/working-hours", WorkingHoursEndpoint)
            .WithName("WorkingHoursEndpoint")
            .WithSummary("Solo accesible durante horario laboral")
            .WithDescription("Demuestra WorkingHoursRequirement (8:00-18:00)")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);

        // Endpoint con validación de departamento
        group.MapGet("/department-access", DepartmentAccessEndpoint)
            .WithName("DepartmentAccessEndpoint")
            .WithSummary("Solo para departamentos específicos")
            .WithDescription("Demuestra DepartmentRequirement (IT, HR, Finance)")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);

        // 7. Endpoint de ejemplo de autorización condicional - demostración combinada
        group.MapGet("/conditional-access", CustomConditionalAccessEndpoint)
            .WithName("CustomConditionalAccessEndpoint")
            .WithSummary("Acceso condicional con múltiples validaciones")
            .WithDescription("Demuestra ConditionalAccessRequirement (MFA + horario + nivel seguridad)")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);

        // Endpoint para gestión de usuarios con política compleja
        group.MapGet("/user-management", UserManagementEndpoint)
            .WithName("UserManagementEndpoint")
            .WithSummary("Gestión de usuarios con política compleja")
            .WithDescription("Combina roles, permisos, departamento y horario")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);

        // Endpoint para reportes financieros con alta seguridad
        group.MapGet("/financial-reports", FinancialReportsEndpoint)
            .WithName("FinancialReportsEndpoint")
            .WithSummary("Reportes financieros con máxima seguridad")
            .WithDescription("MFA + permisos específicos + departamento + token fresco")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);

        // Endpoint para demostrar fallos de autorización con información detallada
        group.MapGet("/authorization-test", AuthorizationTestEndpoint)
            .WithName("AuthorizationTestEndpoint")
            .WithSummary("Prueba de autorización con información detallada")
            .WithDescription("Muestra el estado de todas las validaciones personalizadas")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .RequireAuthorization();
    }

    /// <summary>
    /// Endpoint con validación de permiso personalizado dinámico
    /// </summary>
    private static async Task<IResult> CustomPermissionEndpoint(
        string permission,
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        // Crear política dinámicamente basada en el parámetro
        var policy = PolicyBuilderExtensions.BasicPermissionPolicy(permission);

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

    /// <summary>
    /// Endpoint con validación de múltiples permisos
    /// </summary>
    private static async Task<IResult> MultiplePermissionsEndpoint(
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        var policy = PolicyBuilderExtensions.NewPolicy()
            .RequireAllPermissions("adminaccess", "systemconfigwrite")
            .Build();

        var authResult = await authorizationService.AuthorizeAsync(context.User, policy);

        if (!authResult.Succeeded)
        {
            var failures = authResult.Failure?.FailureReasons?.Select(r => r.Message) ?? new[] { "Acceso denegado" };
            return Results.Json(new ApiResponse
            {
                Success = false,
                Message = "No tienes todos los permisos requeridos (adminaccess Y systemconfigwrite)",
                Errors = failures.ToList()
            }, statusCode: StatusCodes.Status403Forbidden);
        }

        var username = context.User.Identity?.Name ?? "Usuario";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var response = new
        {
            Message = $"Operación crítica autorizada para {username}",
            RequiredPermissions = new[] { "adminaccess", "systemconfigwrite" },
            CustomHandler = "MultiplePermissionsAuthorizationHandler",
            UserPermissions = userPermissions,
            CriticalOperation = new
            {
                Type = "SystemConfigurationUpdate",
                Description = "Actualización crítica de configuración del sistema",
                RequiresMultiplePermissions = true,
                ValidationLogic = "Todos los permisos deben estar presentes (AND lógico)"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Múltiples permisos validados exitosamente",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint con validación de permisos alternativos
    /// </summary>
    private static async Task<IResult> AnyPermissionEndpoint(
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        var policy = PolicyBuilderExtensions.NewPolicy()
            .RequireAnyPermission("usersread", "reportsread", "adminaccess")
            .Build();

        var authResult = await authorizationService.AuthorizeAsync(context.User, policy);

        if (!authResult.Succeeded)
        {
            return Results.Json(new ApiResponse
            {
                Success = false,
                Message = "No tienes ninguno de los permisos requeridos (usersread O reportsread O adminaccess)",
                Errors = new List<string> { "Permiso insuficiente" }
            }, statusCode: StatusCodes.Status403Forbidden);
        }

        var username = context.User.Identity?.Name ?? "Usuario";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();
        var acceptedPermissions = new[] { "usersread", "reportsread", "adminaccess" };
        var matchingPermissions = userPermissions.Intersect(acceptedPermissions).ToList();

        var response = new
        {
            Message = $"Acceso alternativo autorizado para {username}",
            AcceptedPermissions = acceptedPermissions,
            UserMatchingPermissions = matchingPermissions,
            CustomHandler = "AnyPermissionAuthorizationHandler",
            FlexibleAccess = new
            {
                ValidationLogic = "Al menos uno de los permisos debe estar presente (OR lógico)",
                AccessReason = $"Usuario tiene: {string.Join(", ", matchingPermissions)}",
                Scalability = "Fácil agregar nuevos permisos alternativos"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Permiso alternativo validado exitosamente",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint con validación híbrida de roles y permisos
    /// </summary>
    private static async Task<IResult> HybridValidationEndpoint(
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        var policy = PolicyBuilderExtensions.NewPolicy()
            .RequireRoleWithPermissions(
                roles: new[] { "Manager", "Admin", "SuperAdmin" },
                permissions: new[] { "usersread", "reportsread" })
            .Build();

        var authResult = await authorizationService.AuthorizeAsync(context.User, policy);

        if (!authResult.Succeeded)
        {
            return Results.Json(new ApiResponse
            {
                Success = false,
                Message = "No cumples con los requisitos híbridos (rol gerencial + permisos específicos)",
                Errors = new List<string> { "Autorización híbrida falló" }
            }, statusCode: StatusCodes.Status403Forbidden);
        }

        var username = context.User.Identity?.Name ?? "Usuario";
        var userRoles = context.User.FindAll("role").Select(c => c.Value).ToList();
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        var response = new
        {
            Message = $"Validación híbrida exitosa para {username}",
            RequiredRoles = new[] { "Manager", "Admin", "SuperAdmin" },
            RequiredPermissions = new[] { "usersread", "reportsread" },
            UserRoles = userRoles,
            UserPermissions = userPermissions,
            CustomHandler = "RoleWithPermissionAuthorizationHandler",
            HybridLogic = new
            {
                RoleValidation = "Usuario debe tener AL MENOS UNO de los roles requeridos",
                PermissionValidation = "Usuario debe tener TODOS los permisos requeridos",
                CombinedLogic = "Ambas validaciones deben pasar (role OR permission) AND (permission1 AND permission2)",
                UseCase = "Ideal para operaciones que requieren nivel organizacional + capacidades específicas"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Autorización híbrida completada",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint con validación de horario laboral
    /// </summary>
    private static async Task<IResult> WorkingHoursEndpoint(
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        var policy = PolicyBuilderExtensions.NewPolicy()
            .RequireWorkingHours(new TimeSpan(8, 0, 0), new TimeSpan(18, 0, 0), allowAdminBypass: true)
            .Build();

        var authResult = await authorizationService.AuthorizeAsync(context.User, policy);

        if (!authResult.Succeeded)
        {
            var failures = authResult.Failure?.FailureReasons?.Select(r => r.Message) ?? new[] { "Fuera de horario laboral" };
            return Results.Json(new ApiResponse
            {
                Success = false,
                Message = "Acceso solo permitido durante horario laboral (8:00 - 18:00)",
                Errors = failures.ToList()
            }, statusCode: StatusCodes.Status403Forbidden);
        }

        var username = context.User.Identity?.Name ?? "Usuario";
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();
        var hasAdminBypass = userPermissions.Contains("adminaccess");

        var utcNow = DateTime.UtcNow;
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZoneInfo.Local);

        var response = new
        {
            Message = $"Acceso durante horario laboral autorizado para {username}",
            CustomHandler = "WorkingHoursAuthorizationHandler",
            WorkingHours = new
            {
                StartTime = "08:00",
                EndTime = "18:00",
                TimeZone = TimeZoneInfo.Local.Id,
                CurrentTime = localTime.ToString("HH:mm"),
                IsWorkingHours = true
            },
            AdminBypass = new
            {
                Enabled = true,
                UserHasAdminAccess = hasAdminBypass,
                BypassUsed = hasAdminBypass && (localTime.TimeOfDay < new TimeSpan(8, 0, 0) || localTime.TimeOfDay > new TimeSpan(18, 0, 0))
            },
            SecurityFeatures = new
            {
                TemporalAccess = "Acceso restringido por tiempo",
                BusinessHours = "Solo durante horario comercial",
                AdminOverride = "Admins pueden acceder fuera de horario",
                UseCase = "Operaciones sensibles restringidas por horario"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Validación de horario laboral exitosa",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint con validación de departamento
    /// </summary>
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

    /// <summary>
    /// Endpoint con acceso condicional complejo
    /// </summary>
    private static async Task<IResult> CustomConditionalAccessEndpoint(
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        var policy = PolicyBuilderExtensions.AdvancedConditionalPolicy();

        var authResult = await authorizationService.AuthorizeAsync(context.User, policy);

        if (!authResult.Succeeded)
        {
            var failures = authResult.Failure?.FailureReasons?.Select(r => r.Message) ?? new[] { "Acceso condicional falló" };
            return Results.Json(new ApiResponse
            {
                Success = false,
                Message = "No cumples con los requisitos de acceso condicional",
                Errors = failures.ToList()
            }, statusCode: StatusCodes.Status403Forbidden);
        }

        var username = context.User.Identity?.Name ?? "Usuario";

        var response = new
        {
            Message = $"Acceso condicional autorizado para {username}",
            CustomHandler = "ConditionalAccessAuthorizationHandler",
            ConditionalRequirements = new
            {
                MfaRequired = true,
                MaxTokenAge = "60 minutos",
                WorkingHoursRequired = true,
                MinimumSecurityLevel = "Medium"
            },
            ValidationResults = AnalyzeConditionalAccess(context.User),
            SecurityBenefits = new
            {
                MultiFactorSecurity = "Múltiples validaciones simultáneas",
                ContextAware = "Considera tiempo, ubicación, y estado del usuario",
                Adaptive = "Puede ajustar requisitos según riesgo",
                ZeroTrust = "Valida continuamente, no solo en login"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Acceso condicional validado exitosamente",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint para gestión de usuarios con política compleja
    /// </summary>
    private static async Task<IResult> UserManagementEndpoint(
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        var policy = PolicyBuilderExtensions.UserManagementPolicy();

        var authResult = await authorizationService.AuthorizeAsync(context.User, policy);

        if (!authResult.Succeeded)
        {
            var failures = authResult.Failure?.FailureReasons?.Select(r => r.Message) ?? new[] { "Autorización de gestión falló" };
            return Results.Json(new ApiResponse
            {
                Success = false,
                Message = "No tienes autorización para gestión de usuarios",
                Errors = failures.ToList()
            }, statusCode: StatusCodes.Status403Forbidden);
        }

        var username = context.User.Identity?.Name ?? "Usuario";
        var userRoles = context.User.FindAll("role").Select(c => c.Value).ToList();
        var userDepartment = context.User.FindFirst("department")?.Value ?? "No asignado";

        var response = new
        {
            Message = $"Gestión de usuarios autorizada para {username}",
            ComplexPolicy = "UserManagementPolicy",
            ValidationComponents = new
            {
                RoleRequirement = "Manager, Admin, o SuperAdmin",
                PermissionRequirement = "usersread",
                DepartmentRequirement = "IT, HR, o Management",
                TimeRequirement = "07:00 - 19:00"
            },
            UserValidation = new
            {
                UserRoles = userRoles,
                UserDepartment = userDepartment,
                CurrentTime = DateTime.Now.ToString("HH:mm"),
                AllValidationsPassed = true
            },
            AvailableOperations = new[]
            {
                "Ver lista de usuarios",
                "Crear nuevos usuarios",
                "Modificar información de usuarios",
                "Asignar roles y permisos",
                "Generar reportes de usuarios"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Política compleja de gestión validada",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint para reportes financieros con alta seguridad
    /// </summary>
    private static async Task<IResult> FinancialReportsEndpoint(
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        var policy = PolicyBuilderExtensions.FinancialReportsPolicy();

        var authResult = await authorizationService.AuthorizeAsync(context.User, policy);

        if (!authResult.Succeeded)
        {
            var failures = authResult.Failure?.FailureReasons?.Select(r => r.Message) ?? new[] { "Acceso a reportes financieros denegado" };
            return Results.Json(new ApiResponse
            {
                Success = false,
                Message = "Acceso denegado: Reportes financieros requieren máxima seguridad",
                Errors = failures.ToList()
            }, statusCode: StatusCodes.Status403Forbidden);
        }

        var username = context.User.Identity?.Name ?? "Usuario";
        var userDepartment = context.User.FindFirst("department")?.Value ?? "No asignado";

        var response = new
        {
            Message = $"Acceso a reportes financieros autorizado para {username}",
            HighSecurityPolicy = "FinancialReportsPolicy",
            SecurityRequirements = new
            {
                MultiplePermissions = "reports.read Y admin.access",
                RestrictedDepartments = "Finance, Accounting, Executive",
                MfaRequired = true,
                FreshToken = "Máximo 30 minutos",
                HighSecurityLevel = "Nivel mínimo: High"
            },
            AvailableReports = new[]
            {
                new { Name = "Balance General", Sensitivity = "Alto", RequiresMfa = true },
                new { Name = "Estado de Resultados", Sensitivity = "Alto", RequiresMfa = true },
                new { Name = "Flujo de Caja", Sensitivity = "Crítico", RequiresMfa = true },
                new { Name = "Análisis de Rentabilidad", Sensitivity = "Crítico", RequiresMfa = true }
            },
            AuditTrail = new
            {
                AccessedBy = username,
                Department = userDepartment,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                SecurityLevel = "Maximum",
                ComplianceLogged = true
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Acceso de alta seguridad validado",
            Data = response
        });
    }

    /// <summary>
    /// Endpoint para prueba de autorización con información detallada
    /// </summary>
    private static IResult AuthorizationTestEndpoint(HttpContext context)
    {
        var username = context.User.Identity?.Name ?? "Usuario";
        var userRoles = context.User.FindAll("role").Select(c => c.Value).ToList();
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();
        var userDepartment = context.User.FindFirst("department")?.Value;

        var response = new
        {
            Message = $"Análisis de autorización para {username}",
            UserInfo = new
            {
                Username = username,
                Roles = userRoles,
                Permissions = userPermissions,
                Department = userDepartment ?? "No asignado"
            },
            AuthorizationCapabilities = new
            {
                CanAccessBasicPermissions = userPermissions.Any(),
                CanAccessMultiplePermissions = userPermissions.Contains("adminaccess") && userPermissions.Contains("systemconfigwrite"),
                CanAccessAnyPermission = userPermissions.Intersect(new[] { "usersread", "reportsread", "adminaccess" }).Any(),
                CanAccessHybridValidation = userRoles.Intersect(new[] { "Manager", "Admin", "SuperAdmin" }).Any() &&
                                          userPermissions.Contains("usersread") && userPermissions.Contains("reportsread"),
                CanAccessDepartmental = new[] { "IT", "HR", "Finance" }.Contains(userDepartment, StringComparer.OrdinalIgnoreCase),
                CanAccessFinancialReports = userPermissions.Contains("reportsread") && userPermissions.Contains("adminaccess") &&
                                          new[] { "Finance", "Accounting", "Executive" }.Contains(userDepartment, StringComparer.OrdinalIgnoreCase)
            },
            CustomHandlersAvailable = new[]
            {
                "PermissionAuthorizationHandler",
                "MultiplePermissionsAuthorizationHandler",
                "AnyPermissionAuthorizationHandler",
                "RoleWithPermissionAuthorizationHandler",
                "WorkingHoursAuthorizationHandler",
                "DepartmentAuthorizationHandler",
                "ConditionalAccessAuthorizationHandler"
            },
            PolicyExamples = new[]
            {
                "BasicPermissionPolicy - Permiso individual",
                "AdminPolicy - Acceso administrativo",
                "CriticalOperationPolicy - Operaciones críticas",
                "UserManagementPolicy - Gestión de usuarios",
                "FinancialReportsPolicy - Reportes financieros"
            }
        };

        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Análisis de autorización completado",
            Data = response
        });
    }

    #region Helper Methods

    /// <summary>
    /// Obtiene las operaciones disponibles según el departamento
    /// </summary>
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

    /// <summary>
    /// Analiza el estado del acceso condicional
    /// </summary>
    private static object AnalyzeConditionalAccess(System.Security.Claims.ClaimsPrincipal user)
    {
        var mfaClaim = user.FindFirst("mfa")?.Value;
        var issuedAt = user.FindFirst("iat")?.Value;
        var currentTime = DateTime.Now;

        var tokenAge = "No disponible";
        if (long.TryParse(issuedAt, out long iat))
        {
            var tokenIssuedTime = DateTimeOffset.FromUnixTimeSeconds(iat);
            var age = DateTime.UtcNow - tokenIssuedTime.UtcDateTime;
            tokenAge = $"{age.TotalMinutes:F0} minutos";
        }

        return new
        {
            MfaStatus = mfaClaim == "true" ? "Activado" : "No activado",
            TokenAge = tokenAge,
            CurrentTime = currentTime.ToString("HH:mm"),
            IsWorkingHours = currentTime.TimeOfDay >= new TimeSpan(8, 0, 0) && currentTime.TimeOfDay <= new TimeSpan(18, 0, 0),
            SecurityLevel = GetUserSecurityLevel(user)
        };
    }

    /// <summary>
    /// Obtiene el nivel de seguridad del usuario
    /// </summary>
    private static string GetUserSecurityLevel(System.Security.Claims.ClaimsPrincipal user)
    {
        var roles = user.FindAll("role").Select(c => c.Value).ToList();
        var permissions = user.FindAll("permissions").Select(c => c.Value).ToList();

        if (roles.Contains("SuperAdmin")) return "Maximum";
        if (roles.Contains("Admin") || permissions.Contains("adminaccess")) return "High";
        if (roles.Contains("Manager")) return "Medium";
        if (permissions.Any(p => p.Contains("write"))) return "Medium-Low";
        return "Basic";
    }

    #endregion
}
