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
