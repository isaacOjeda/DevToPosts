using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Requirements;

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
