using AdvancedAuthorization.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AdvancedAuthorization.Authorization.Handlers;

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
