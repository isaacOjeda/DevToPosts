namespace AdvancedAuthorization.Models.Constants;

/// <summary>
/// Constantes relacionadas con la autorización del sistema
/// </summary>
public static class AuthConstants
{
    /// <summary>
    /// Tipos de claims personalizados del sistema
    /// </summary>
    public static class ClaimTypes
    {
        /// <summary>
        /// Claim que contiene los permisos del usuario
        /// </summary>
        public const string Permissions = "permissions";

        /// <summary>
        /// Claim que contiene el departamento del usuario
        /// </summary>
        public const string Department = "department";

        /// <summary>
        /// Claim que contiene el ID del usuario
        /// </summary>
        public const string UserId = "user_id";

        /// <summary>
        /// Claim que contiene el nombre completo del usuario
        /// </summary>
        public const string FullName = "full_name";

        /// <summary>
        /// Claim que indica si el usuario está activo
        /// </summary>
        public const string IsActive = "is_active";
    }

    /// <summary>
    /// Nombres de políticas de autorización predefinidas
    /// </summary>
    public static class Policies
    {
        /// <summary>
        /// Política para acceso de solo administradores
        /// </summary>
        public const string AdminOnly = "AdminOnly";

        /// <summary>
        /// Política para acceso de managers y superiores
        /// </summary>
        public const string ManagerOrAbove = "ManagerOrAbove";

        /// <summary>
        /// Política para usuarios activos
        /// </summary>
        public const string ActiveUser = "ActiveUser";

        /// <summary>
        /// Política para acceso a usuarios
        /// </summary>
        public const string CanReadUsers = "CanReadUsers";
        public const string CanWriteUsers = "CanWriteUsers";
        public const string CanDeleteUsers = "CanDeleteUsers";

        /// <summary>
        /// Política para acceso a reportes
        /// </summary>
        public const string CanReadReports = "CanReadReports";
        public const string CanWriteReports = "CanWriteReports";
        public const string CanDeleteReports = "CanDeleteReports";

        /// <summary>
        /// Política para acceso administrativo
        /// </summary>
        public const string AdminAccess = "AdminAccess";
        public const string AdminSettings = "AdminSettings";

        /// <summary>
        /// Política para gestión de roles
        /// </summary>
        public const string CanReadRoles = "CanReadRoles";
        public const string CanWriteRoles = "CanWriteRoles";
        public const string CanDeleteRoles = "CanDeleteRoles";

        /// <summary>
        /// Política para auditoría
        /// </summary>
        public const string CanReadAudit = "CanReadAudit";
        public const string CanWriteAudit = "CanWriteAudit";

        /// <summary>
        /// Política para configuración del sistema
        /// </summary>
        public const string CanReadSystemConfig = "CanReadSystemConfig";
        public const string CanWriteSystemConfig = "CanWriteSystemConfig";
    }

    /// <summary>
    /// Valores por defecto para configuración JWT
    /// </summary>
    public static class JwtDefaults
    {
        /// <summary>
        /// Tiempo de expiración por defecto en minutos
        /// </summary>
        public const int DefaultExpiryMinutes = 60;

        /// <summary>
        /// Issuer por defecto
        /// </summary>
        public const string DefaultIssuer = "AdvancedAuthorizationAPI";

        /// <summary>
        /// Audience por defecto
        /// </summary>
        public const string DefaultAudience = "AdvancedAuthorizationClients";
    }

    /// <summary>
    /// Mapeo entre permisos y nombres de políticas
    /// </summary>
    public static readonly Dictionary<Permission, string> PermissionToPolicyMap = new()
    {
        [Permission.UsersRead] = Policies.CanReadUsers,
        [Permission.UsersWrite] = Policies.CanWriteUsers,
        [Permission.UsersDelete] = Policies.CanDeleteUsers,

        [Permission.ReportsRead] = Policies.CanReadReports,
        [Permission.ReportsWrite] = Policies.CanWriteReports,
        [Permission.ReportsDelete] = Policies.CanDeleteReports,

        [Permission.AdminAccess] = Policies.AdminAccess,
        [Permission.AdminSettings] = Policies.AdminSettings,

        [Permission.RolesRead] = Policies.CanReadRoles,
        [Permission.RolesWrite] = Policies.CanWriteRoles,
        [Permission.RolesDelete] = Policies.CanDeleteRoles,

        [Permission.AuditRead] = Policies.CanReadAudit,
        [Permission.AuditWrite] = Policies.CanWriteAudit,

        [Permission.SystemConfigRead] = Policies.CanReadSystemConfig,
        [Permission.SystemConfigWrite] = Policies.CanWriteSystemConfig
    };

    /// <summary>
    /// Mapeo inverso: de nombre de política a permiso
    /// </summary>
    public static readonly Dictionary<string, Permission> PolicyToPermissionMap =
        PermissionToPolicyMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
}
