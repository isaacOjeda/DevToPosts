using AdvancedAuthorization.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AdvancedAuthorization.Authorization.Handlers;

/// <summary>
/// Handler para validar permisos específicos
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    /// <summary>
    /// Maneja la validación del requirement
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Buscar el claim de permiso en el usuario
        var permissionClaims = context.User.FindAll("permissions").Select(c => c.Value).ToList();

        // Verificar si el usuario tiene el permiso requerido
        if (permissionClaims.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler para validar múltiples permisos (todos requeridos)
/// </summary>
public class MultiplePermissionsAuthorizationHandler : AuthorizationHandler<MultiplePermissionsRequirement>
{
    /// <summary>
    /// Maneja la validación del requirement
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MultiplePermissionsRequirement requirement)
    {
        // Obtener todos los permisos del usuario
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToHashSet();

        // Verificar que el usuario tenga TODOS los permisos requeridos
        bool hasAllPermissions = requirement.Permissions.All(permission => userPermissions.Contains(permission));

        if (hasAllPermissions)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler para validar permisos alternativos (cualquiera válido)
/// </summary>
public class AnyPermissionAuthorizationHandler : AuthorizationHandler<AnyPermissionRequirement>
{
    /// <summary>
    /// Maneja la validación del requirement
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyPermissionRequirement requirement)
    {
        // Obtener todos los permisos del usuario
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToHashSet();

        // Verificar que el usuario tenga AL MENOS UNO de los permisos requeridos
        bool hasAnyPermission = requirement.Permissions.Any(permission => userPermissions.Contains(permission));

        if (hasAnyPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler para validar combinación de roles y permisos
/// </summary>
public class RoleWithPermissionAuthorizationHandler : AuthorizationHandler<RoleWithPermissionRequirement>
{
    /// <summary>
    /// Maneja la validación del requirement
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleWithPermissionRequirement requirement)
    {
        // Verificar roles
        var userRoles = context.User.FindAll("role").Select(c => c.Value).ToHashSet();
        bool hasRequiredRole = requirement.RequiredRoles.Any(role => userRoles.Contains(role));

        // Verificar permisos
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToHashSet();
        bool hasAllPermissions = requirement.RequiredPermissions.All(permission => userPermissions.Contains(permission));

        // Ambas condiciones deben cumplirse
        if (hasRequiredRole && hasAllPermissions)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler para validar horario de trabajo
/// </summary>
public class WorkingHoursAuthorizationHandler : AuthorizationHandler<WorkingHoursRequirement>
{
    /// <summary>
    /// Maneja la validación del requirement
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkingHoursRequirement requirement)
    {
        // Verificar bypass administrativo
        if (requirement.AllowAdminBypass)
        {
            var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToList();
            if (userPermissions.Contains("adminaccess"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        // Obtener la hora actual en la zona horaria especificada
        var utcNow = DateTime.UtcNow;
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, requirement.TimeZone);
        var currentTime = localTime.TimeOfDay;

        // Verificar si está dentro del horario laboral
        bool isWithinWorkingHours = currentTime >= requirement.StartTime && currentTime <= requirement.EndTime;

        if (isWithinWorkingHours)
        {
            context.Succeed(requirement);
        }
        else
        {
            // Proporcionar información útil sobre por qué falló
            context.Fail(new AuthorizationFailureReason(
                this,
                $"Acceso fuera del horario laboral. Horario permitido: {requirement.StartTime:hh\\:mm} - {requirement.EndTime:hh\\:mm} ({requirement.TimeZone.Id}). Hora actual: {currentTime:hh\\:mm}"));
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler para validar departamento del usuario
/// </summary>
public class DepartmentAuthorizationHandler : AuthorizationHandler<DepartmentRequirement>
{
    /// <summary>
    /// Maneja la validación del requirement
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DepartmentRequirement requirement)
    {
        // Obtener el departamento del usuario
        var userDepartment = context.User.FindFirst("department")?.Value;

        // Si no tiene departamento
        if (string.IsNullOrEmpty(userDepartment))
        {
            if (requirement.AllowNoDepartment)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail(new AuthorizationFailureReason(
                    this,
                    "Usuario no tiene departamento asignado y se requiere uno de los siguientes: " +
                    string.Join(", ", requirement.AllowedDepartments)));
            }
            return Task.CompletedTask;
        }

        // Verificar si el departamento está permitido
        if (requirement.AllowedDepartments.Contains(userDepartment, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(
                this,
                $"Departamento '{userDepartment}' no está autorizado. Departamentos permitidos: {string.Join(", ", requirement.AllowedDepartments)}"));
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler para validar acceso condicional complejo
/// </summary>
public class ConditionalAccessAuthorizationHandler : AuthorizationHandler<ConditionalAccessRequirement>
{
    /// <summary>
    /// Maneja la validación del requirement
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ConditionalAccessRequirement requirement)
    {
        var conditions = requirement.Conditions;
        var failures = new List<string>();

        // Validar MFA si se requiere
        if (conditions.RequireMfa)
        {
            var mfaClaim = context.User.FindFirst("mfa")?.Value;
            if (mfaClaim != "true")
            {
                failures.Add("Se requiere autenticación multifactor (MFA)");
            }
        }

        // Validar edad del token
        if (conditions.MaxTokenAgeMinutes.HasValue)
        {
            var issuedAt = context.User.FindFirst("iat")?.Value;
            if (long.TryParse(issuedAt, out long iat))
            {
                var tokenAge = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - iat;
                var maxAgeSeconds = conditions.MaxTokenAgeMinutes.Value * 60;

                if (tokenAge > maxAgeSeconds)
                {
                    failures.Add($"Token demasiado antiguo. Máximo permitido: {conditions.MaxTokenAgeMinutes} minutos");
                }
            }
            else
            {
                failures.Add("No se puede determinar la edad del token");
            }
        }

        // Validar horario laboral
        if (conditions.RequireWorkingHours)
        {
            var utcNow = DateTime.UtcNow;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZoneInfo.Local);
            var currentTime = localTime.TimeOfDay;

            var workStart = new TimeSpan(8, 0, 0);  // 8:00 AM
            var workEnd = new TimeSpan(18, 0, 0);   // 6:00 PM

            if (currentTime < workStart || currentTime > workEnd)
            {
                failures.Add("Acceso permitido solo durante horario laboral (8:00 - 18:00)");
            }
        }

        // Validar departamento
        if (conditions.AllowedDepartments?.Length > 0)
        {
            var userDepartment = context.User.FindFirst("department")?.Value;
            if (string.IsNullOrEmpty(userDepartment) ||
                !conditions.AllowedDepartments.Contains(userDepartment, StringComparer.OrdinalIgnoreCase))
            {
                failures.Add($"Departamento no autorizado. Permitidos: {string.Join(", ", conditions.AllowedDepartments)}");
            }
        }

        // Validar nivel de seguridad
        if (!string.IsNullOrEmpty(conditions.MinimumSecurityLevel))
        {
            var userSecurityLevel = GetUserSecurityLevel(context.User);
            if (!IsSecurityLevelSufficient(userSecurityLevel, conditions.MinimumSecurityLevel))
            {
                failures.Add($"Nivel de seguridad insuficiente. Requerido: {conditions.MinimumSecurityLevel}, Usuario: {userSecurityLevel}");
            }
        }

        // Evaluar resultado
        if (failures.Count == 0)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(
                this,
                $"Acceso condicional falló: {string.Join("; ", failures)}"));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Obtiene el nivel de seguridad del usuario basado en sus roles y permisos
    /// </summary>
    private string GetUserSecurityLevel(ClaimsPrincipal user)
    {
        var roles = user.FindAll("role").Select(c => c.Value).ToList();
        var permissions = user.FindAll("permissions").Select(c => c.Value).ToList();

        if (roles.Contains("SuperAdmin")) return "Maximum";
        if (roles.Contains("Admin") || permissions.Contains("adminaccess")) return "High";
        if (roles.Contains("Manager")) return "Medium";
        if (permissions.Any(p => p.Contains("write"))) return "Medium-Low";
        return "Basic";
    }

    /// <summary>
    /// Verifica si el nivel de seguridad del usuario es suficiente
    /// </summary>
    private bool IsSecurityLevelSufficient(string userLevel, string requiredLevel)
    {
        var levels = new Dictionary<string, int>
        {
            ["Basic"] = 1,
            ["Medium-Low"] = 2,
            ["Medium"] = 3,
            ["High"] = 4,
            ["Maximum"] = 5
        };

        var userLevelValue = levels.GetValueOrDefault(userLevel, 0);
        var requiredLevelValue = levels.GetValueOrDefault(requiredLevel, 0);

        return userLevelValue >= requiredLevelValue;
    }
}
