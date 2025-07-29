using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Requirements;

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
