using AdvancedAuthorization.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Services;

/// <summary>
/// Servicio para construir políticas de autorización de forma fluida
/// </summary>
public interface IPolicyBuilder
{
    /// <summary>
    /// Requiere un permiso específico
    /// </summary>
    IPolicyBuilder RequirePermission(string permission);

    /// <summary>
    /// Requiere múltiples permisos (todos)
    /// </summary>
    IPolicyBuilder RequireAllPermissions(params string[] permissions);

    /// <summary>
    /// Requiere cualquiera de los permisos especificados
    /// </summary>
    IPolicyBuilder RequireAnyPermission(params string[] permissions);

    /// <summary>
    /// Requiere un rol específico con permisos
    /// </summary>
    IPolicyBuilder RequireRoleWithPermissions(string[] roles, string[] permissions);

    /// <summary>
    /// Requiere horario laboral
    /// </summary>
    IPolicyBuilder RequireWorkingHours(TimeSpan startTime, TimeSpan endTime, bool allowAdminBypass = true);

    /// <summary>
    /// Requiere departamento específico
    /// </summary>
    IPolicyBuilder RequireDepartment(params string[] departments);

    /// <summary>
    /// Requiere acceso condicional
    /// </summary>
    IPolicyBuilder RequireConditionalAccess(Action<ConditionalAccessConditions> configure);

    /// <summary>
    /// Construye la política de autorización
    /// </summary>
    AuthorizationPolicy Build();
}
