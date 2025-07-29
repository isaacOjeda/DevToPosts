namespace AdvancedAuthorization.Models;

/// <summary>
/// Define los roles básicos del sistema.
/// Los roles representan conjuntos de permisos que se asignan a los usuarios.
/// </summary>
public enum Role
{
    /// <summary>
    /// Usuario básico con permisos mínimos
    /// </summary>
    User,

    /// <summary>
    /// Manager con permisos intermedios
    /// </summary>
    Manager,

    /// <summary>
    /// Administrador con permisos completos
    /// </summary>
    Admin,

    /// <summary>
    /// Super administrador con acceso total al sistema
    /// </summary>
    SuperAdmin,

    /// <summary>
    /// Auditor con permisos de solo lectura para auditoría
    /// </summary>
    Auditor
}

/// <summary>
/// Extension methods para trabajar con roles
/// </summary>
public static class RoleExtensions
{
    /// <summary>
    /// Convierte el rol a su representación string para usar en claims
    /// </summary>
    public static string ToClaimValue(this Role role)
    {
        return role.ToString();
    }

    /// <summary>
    /// Obtiene una descripción del rol
    /// </summary>
    public static string GetDescription(this Role role)
    {
        return role switch
        {
            Role.User => "Usuario básico del sistema",
            Role.Manager => "Manager con permisos de gestión",
            Role.Admin => "Administrador del sistema",
            Role.SuperAdmin => "Super administrador con acceso completo",
            Role.Auditor => "Auditor con permisos de lectura",
            _ => role.ToString()
        };
    }

    /// <summary>
    /// Obtiene los permisos predeterminados para cada rol
    /// </summary>
    public static Permission[] GetDefaultPermissions(this Role role)
    {
        return role switch
        {
            Role.User => new[]
            {
                Permission.UsersRead,
                Permission.ReportsRead
            },

            Role.Manager => new[]
            {
                Permission.UsersRead,
                Permission.UsersWrite,
                Permission.ReportsRead,
                Permission.ReportsWrite,
                Permission.RolesRead
            },

            Role.Admin => new[]
            {
                Permission.UsersRead,
                Permission.UsersWrite,
                Permission.UsersDelete,
                Permission.ReportsRead,
                Permission.ReportsWrite,
                Permission.ReportsDelete,
                Permission.AdminAccess,
                Permission.AdminSettings,
                Permission.RolesRead,
                Permission.RolesWrite,
                Permission.SystemConfigRead
            },

            Role.SuperAdmin => Enum.GetValues<Permission>(), // Todos los permisos

            Role.Auditor => new[]
            {
                Permission.UsersRead,
                Permission.ReportsRead,
                Permission.RolesRead,
                Permission.AuditRead,
                Permission.SystemConfigRead
            },

            _ => Array.Empty<Permission>()
        };
    }
}
