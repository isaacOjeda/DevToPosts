namespace AdvancedAuthorization.Models;

/// <summary>
/// Define los permisos específicos del sistema de autorización granular.
/// Cada permiso representa una acción específica que puede ser autorizada.
/// </summary>
public enum Permission
{
    // Permisos de usuarios
    UsersRead,
    UsersWrite,
    UsersDelete,

    // Permisos de reportes
    ReportsRead,
    ReportsWrite,
    ReportsDelete,

    // Permisos administrativos
    AdminAccess,
    AdminSettings,

    // Permisos de gestión de roles
    RolesRead,
    RolesWrite,
    RolesDelete,

    // Permisos de auditoría
    AuditRead,
    AuditWrite,

    // Permisos de configuración del sistema
    SystemConfigRead,
    SystemConfigWrite
}

/// <summary>
/// Extension methods para trabajar con permisos de manera más fluida
/// </summary>
public static class PermissionExtensions
{
    /// <summary>
    /// Convierte el permiso a su representación string para usar en claims
    /// </summary>
    public static string ToClaimValue(this Permission permission)
    {
        return permission.GetInvariantName();
    }

    /// <summary>
    /// Obtiene una descripción amigable del permiso
    /// </summary>
    public static string GetDescription(this Permission permission)
    {
        return permission switch
        {
            Permission.UsersRead => "Leer información de usuarios",
            Permission.UsersWrite => "Crear y editar usuarios",
            Permission.UsersDelete => "Eliminar usuarios",
            Permission.ReportsRead => "Leer reportes",
            Permission.ReportsWrite => "Crear y editar reportes",
            Permission.ReportsDelete => "Eliminar reportes",
            Permission.AdminAccess => "Acceso a panel administrativo",
            Permission.AdminSettings => "Modificar configuraciones administrativas",
            Permission.RolesRead => "Leer roles del sistema",
            Permission.RolesWrite => "Crear y editar roles",
            Permission.RolesDelete => "Eliminar roles",
            Permission.AuditRead => "Leer registros de auditoría",
            Permission.AuditWrite => "Crear registros de auditoría",
            Permission.SystemConfigRead => "Leer configuración del sistema",
            Permission.SystemConfigWrite => "Modificar configuración del sistema",
            _ => permission.ToString()
        };
    }

    public static string GetInvariantName(this Permission permission)
    {
        return permission switch
        {
            Permission.UsersRead => "users.read",
            Permission.UsersWrite => "users.write",
            Permission.UsersDelete => "users.delete",
            Permission.ReportsRead => "reports.read",
            Permission.ReportsWrite => "reports.write",
            Permission.ReportsDelete => "reports.delete",
            Permission.AdminAccess => "admin.access",
            Permission.AdminSettings => "admin.settings",
            Permission.RolesRead => "roles.read",
            Permission.RolesWrite => "roles.write",
            Permission.RolesDelete => "roles.delete",
            Permission.AuditRead => "audit.read",
            Permission.AuditWrite => "audit.write",
            Permission.SystemConfigRead => "system.config.read",
            Permission.SystemConfigWrite => "system.config.write",
            _ => permission.ToString().ToLowerInvariant()
        };
    }
}
