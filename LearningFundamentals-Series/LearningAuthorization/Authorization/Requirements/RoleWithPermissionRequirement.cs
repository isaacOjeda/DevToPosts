using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Requirements;

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
