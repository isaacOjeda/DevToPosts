namespace AdvancedAuthorization.Models;

/// <summary>
/// Modelo que representa un usuario del sistema
/// </summary>
public class User
{
    /// <summary>
    /// Identificador único del usuario
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de usuario único
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Dirección de correo electrónico
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// Roles asignados al usuario
    /// </summary>
    public List<Role> Roles { get; set; } = new();

    /// <summary>
    /// Permisos específicos asignados directamente al usuario
    /// (adicionales a los que provienen de los roles)
    /// </summary>
    public List<Permission> DirectPermissions { get; set; } = new();

    /// <summary>
    /// Departamento al que pertenece el usuario
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Indica si el usuario está activo
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Fecha de creación del usuario
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Última fecha de inicio de sesión
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Extension methods para el modelo User
/// </summary>
public static class UserExtensions
{
    /// <summary>
    /// Obtiene todos los permisos efectivos del usuario (roles + permisos directos)
    /// </summary>
    public static Permission[] GetEffectivePermissions(this User user)
    {
        var rolePermissions = user.Roles
            .SelectMany(role => role.GetDefaultPermissions())
            .ToHashSet();

        var allPermissions = rolePermissions
            .Union(user.DirectPermissions)
            .ToArray();

        return allPermissions;
    }

    /// <summary>
    /// Verifica si el usuario tiene un permiso específico
    /// </summary>
    public static bool HasPermission(this User user, Permission permission)
    {
        return user.GetEffectivePermissions().Contains(permission);
    }

    /// <summary>
    /// Verifica si el usuario tiene alguno de los permisos especificados
    /// </summary>
    public static bool HasAnyPermission(this User user, params Permission[] permissions)
    {
        var effectivePermissions = user.GetEffectivePermissions();
        return permissions.Any(permission => effectivePermissions.Contains(permission));
    }

    /// <summary>
    /// Verifica si el usuario tiene todos los permisos especificados
    /// </summary>
    public static bool HasAllPermissions(this User user, params Permission[] permissions)
    {
        var effectivePermissions = user.GetEffectivePermissions();
        return permissions.All(permission => effectivePermissions.Contains(permission));
    }

    /// <summary>
    /// Verifica si el usuario tiene un rol específico
    /// </summary>
    public static bool HasRole(this User user, Role role)
    {
        return user.Roles.Contains(role);
    }

    /// <summary>
    /// Verifica si el usuario tiene alguno de los roles especificados
    /// </summary>
    public static bool HasAnyRole(this User user, params Role[] roles)
    {
        return roles.Any(role => user.Roles.Contains(role));
    }
}
