using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Requirements;

/// <summary>
/// Requirement que valida contexto temporal (horario de trabajo)
/// </summary>
public class WorkingHoursRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Hora de inicio del horario laboral
    /// </summary>
    public TimeSpan StartTime { get; }

    /// <summary>
    /// Hora de fin del horario laboral
    /// </summary>
    public TimeSpan EndTime { get; }

    /// <summary>
    /// Zona horaria para validación
    /// </summary>
    public TimeZoneInfo TimeZone { get; }

    /// <summary>
    /// Si se permite bypass con permisos administrativos
    /// </summary>
    public bool AllowAdminBypass { get; }

    /// <summary>
    /// Constructor del requirement
    /// </summary>
    /// <param name="startTime">Hora de inicio (ej: 08:00)</param>
    /// <param name="endTime">Hora de fin (ej: 18:00)</param>
    /// <param name="timeZone">Zona horaria (null = UTC)</param>
    /// <param name="allowAdminBypass">Permitir bypass con admin.access</param>
    public WorkingHoursRequirement(
        TimeSpan startTime,
        TimeSpan endTime,
        TimeZoneInfo? timeZone = null,
        bool allowAdminBypass = true)
    {
        StartTime = startTime;
        EndTime = endTime;
        TimeZone = timeZone ?? TimeZoneInfo.Utc;
        AllowAdminBypass = allowAdminBypass;
    }

    /// <summary>
    /// Descripción del requirement para debugging
    /// </summary>
    public override string ToString()
    {
        return $"WorkingHoursRequirement: {StartTime:hh\\:mm} - {EndTime:hh\\:mm} ({TimeZone.Id})";
    }
}
