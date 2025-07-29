using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Requirements;

/// <summary>
/// Requirement que valida si el usuario tiene un permiso específico
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// El permiso requerido
    /// </summary>
    public string Permission { get; }

    /// <summary>
    /// Constructor del requirement
    /// </summary>
    /// <param name="permission">El permiso que se requiere</param>
    public PermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }

    /// <summary>
    /// Descripción del requirement para debugging
    /// </summary>
    public override string ToString()
    {
        return $"PermissionRequirement: {Permission}";
    }
}

/// <summary>
/// Requirement que valida múltiples permisos (todos requeridos - AND lógico)
/// </summary>
public class MultiplePermissionsRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Los permisos requeridos (todos deben estar presentes)
    /// </summary>
    public IReadOnlyList<string> Permissions { get; }

    /// <summary>
    /// Constructor del requirement
    /// </summary>
    /// <param name="permissions">Los permisos que se requieren (todos)</param>
    public MultiplePermissionsRequirement(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            throw new ArgumentException("Se requiere al menos un permiso", nameof(permissions));

        Permissions = permissions.ToList().AsReadOnly();
    }

    /// <summary>
    /// Descripción del requirement para debugging
    /// </summary>
    public override string ToString()
    {
        return $"MultiplePermissionsRequirement: {string.Join(" AND ", Permissions)}";
    }
}

/// <summary>
/// Requirement que valida permisos alternativos (cualquiera válido - OR lógico)
/// </summary>
public class AnyPermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Los permisos alternativos (cualquiera es suficiente)
    /// </summary>
    public IReadOnlyList<string> Permissions { get; }

    /// <summary>
    /// Constructor del requirement
    /// </summary>
    /// <param name="permissions">Los permisos alternativos (cualquiera)</param>
    public AnyPermissionRequirement(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            throw new ArgumentException("Se requiere al menos un permiso", nameof(permissions));

        Permissions = permissions.ToList().AsReadOnly();
    }

    /// <summary>
    /// Descripción del requirement para debugging
    /// </summary>
    public override string ToString()
    {
        return $"AnyPermissionRequirement: {string.Join(" OR ", Permissions)}";
    }
}

/// <summary>
/// Requirement que combina roles y permisos
/// </summary>
public class RoleWithPermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Los roles requeridos (cualquiera es suficiente)
    /// </summary>
    public IReadOnlyList<string> RequiredRoles { get; }

    /// <summary>
    /// Los permisos requeridos (todos deben estar presentes)
    /// </summary>
    public IReadOnlyList<string> RequiredPermissions { get; }

    /// <summary>
    /// Constructor del requirement
    /// </summary>
    /// <param name="roles">Los roles requeridos</param>
    /// <param name="permissions">Los permisos requeridos</param>
    public RoleWithPermissionRequirement(string[] roles, string[] permissions)
    {
        if (roles == null || roles.Length == 0)
            throw new ArgumentException("Se requiere al menos un rol", nameof(roles));

        if (permissions == null || permissions.Length == 0)
            throw new ArgumentException("Se requiere al menos un permiso", nameof(permissions));

        RequiredRoles = roles.ToList().AsReadOnly();
        RequiredPermissions = permissions.ToList().AsReadOnly();
    }

    /// <summary>
    /// Descripción del requirement para debugging
    /// </summary>
    public override string ToString()
    {
        var rolesStr = string.Join(" OR ", RequiredRoles);
        var permissionsStr = string.Join(" AND ", RequiredPermissions);
        return $"RoleWithPermissionRequirement: ({rolesStr}) AND ({permissionsStr})";
    }
}

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

/// <summary>
/// Requirement que valida el departamento del usuario
/// </summary>
public class DepartmentRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Los departamentos permitidos
    /// </summary>
    public IReadOnlyList<string> AllowedDepartments { get; }

    /// <summary>
    /// Si se permite acceso a usuarios sin departamento asignado
    /// </summary>
    public bool AllowNoDepartment { get; }

    /// <summary>
    /// Constructor del requirement
    /// </summary>
    /// <param name="allowedDepartments">Departamentos permitidos</param>
    /// <param name="allowNoDepartment">Permitir usuarios sin departamento</param>
    public DepartmentRequirement(string[] allowedDepartments, bool allowNoDepartment = false)
    {
        if (allowedDepartments == null || allowedDepartments.Length == 0)
            throw new ArgumentException("Se requiere al menos un departamento", nameof(allowedDepartments));

        AllowedDepartments = allowedDepartments.ToList().AsReadOnly();
        AllowNoDepartment = allowNoDepartment;
    }

    /// <summary>
    /// Descripción del requirement para debugging
    /// </summary>
    public override string ToString()
    {
        var depts = string.Join(", ", AllowedDepartments);
        var noDept = AllowNoDepartment ? " (permite sin departamento)" : "";
        return $"DepartmentRequirement: {depts}{noDept}";
    }
}

/// <summary>
/// Requirement que valida múltiples condiciones con lógica compleja
/// </summary>
public class ConditionalAccessRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Condiciones que deben cumplirse
    /// </summary>
    public ConditionalAccessConditions Conditions { get; }

    /// <summary>
    /// Constructor del requirement
    /// </summary>
    /// <param name="conditions">Las condiciones a evaluar</param>
    public ConditionalAccessRequirement(ConditionalAccessConditions conditions)
    {
        Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
    }

    /// <summary>
    /// Descripción del requirement para debugging
    /// </summary>
    public override string ToString()
    {
        return $"ConditionalAccessRequirement: {Conditions}";
    }
}

/// <summary>
/// Condiciones para acceso condicional
/// </summary>
public class ConditionalAccessConditions
{
    /// <summary>
    /// Requiere MFA (Multi-Factor Authentication)
    /// </summary>
    public bool RequireMfa { get; set; }

    /// <summary>
    /// Edad máxima del token en minutos
    /// </summary>
    public int? MaxTokenAgeMinutes { get; set; }

    /// <summary>
    /// Horario laboral requerido
    /// </summary>
    public bool RequireWorkingHours { get; set; }

    /// <summary>
    /// Departamentos permitidos
    /// </summary>
    public string[]? AllowedDepartments { get; set; }

    /// <summary>
    /// Nivel mínimo de seguridad requerido
    /// </summary>
    public string? MinimumSecurityLevel { get; set; }

    /// <summary>
    /// Descripción de las condiciones
    /// </summary>
    public override string ToString()
    {
        var conditions = new List<string>();

        if (RequireMfa) conditions.Add("MFA requerido");
        if (MaxTokenAgeMinutes.HasValue) conditions.Add($"Token < {MaxTokenAgeMinutes}min");
        if (RequireWorkingHours) conditions.Add("Horario laboral");
        if (AllowedDepartments?.Length > 0) conditions.Add($"Depts: {string.Join(",", AllowedDepartments)}");
        if (!string.IsNullOrEmpty(MinimumSecurityLevel)) conditions.Add($"Security: {MinimumSecurityLevel}");

        return string.Join(" AND ", conditions);
    }
}
