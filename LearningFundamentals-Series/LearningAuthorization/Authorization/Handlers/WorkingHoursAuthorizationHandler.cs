using AdvancedAuthorization.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Handlers;

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
