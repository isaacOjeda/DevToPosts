using AdvancedAuthorization.Models;

namespace AdvancedAuthorization.Services;

/// <summary>
/// Servicio mock para gestión de usuarios
/// </summary>
public class UserService : IUserService
{
    // Base de datos mock de usuarios
    private static readonly List<User> _users = new()
    {
        new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@example.com",
            FullName = "Administrator",
            Department = "IT",
            Roles = new List<Role> { Role.SuperAdmin },
            DirectPermissions = new List<Permission>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            LastLoginAt = DateTime.UtcNow.AddHours(-2)
        },
        new User
        {
            Id = 2,
            Username = "manager",
            Email = "manager@example.com",
            FullName = "John Manager",
            Department = "Operations",
            Roles = new List<Role> { Role.Manager },
            DirectPermissions = new List<Permission> { Permission.AdminAccess }, // Permiso adicional
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            LastLoginAt = DateTime.UtcNow.AddHours(-5)
        },
        new User
        {
            Id = 3,
            Username = "user",
            Email = "user@example.com",
            FullName = "Regular User",
            Department = "Sales",
            Roles = new List<Role> { Role.User },
            DirectPermissions = new List<Permission>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            LastLoginAt = DateTime.UtcNow.AddHours(-1)
        },
        new User
        {
            Id = 4,
            Username = "auditor",
            Email = "auditor@example.com",
            FullName = "System Auditor",
            Department = "Compliance",
            Roles = new List<Role> { Role.Auditor },
            DirectPermissions = new List<Permission>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            LastLoginAt = DateTime.UtcNow.AddDays(-1)
        },
        new User
        {
            Id = 5,
            Username = "itadmin",
            Email = "itadmin@example.com",
            FullName = "IT Administrator",
            Department = "IT",
            Roles = new List<Role> { Role.Admin },
            DirectPermissions = new List<Permission> { Permission.SystemConfigWrite }, // Permiso extra
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-25),
            LastLoginAt = DateTime.UtcNow.AddHours(-3)
        },
        new User
        {
            Id = 6,
            Username = "inactive",
            Email = "inactive@example.com",
            FullName = "Inactive User",
            Department = "HR",
            Roles = new List<Role> { Role.User },
            DirectPermissions = new List<Permission>(),
            IsActive = false, // Usuario inactivo para testing
            CreatedAt = DateTime.UtcNow.AddDays(-50),
            LastLoginAt = DateTime.UtcNow.AddDays(-30)
        }
    };

    // Mock de contraseñas (en producción usar hashing)
    private static readonly Dictionary<string, string> _passwords = new()
    {
        ["admin"] = "admin123",
        ["manager"] = "manager123",
        ["user"] = "user123",
        ["auditor"] = "auditor123",
        ["itadmin"] = "itadmin123",
        ["inactive"] = "inactive123"
    };

    /// <summary>
    /// Autentica un usuario con credenciales
    /// </summary>
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        await Task.Delay(50); // Simular latencia de base de datos

        var user = _users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user == null || !user.IsActive)
        {
            return null;
        }

        // Verificar contraseña (mock)
        if (_passwords.TryGetValue(user.Username, out var storedPassword) &&
            storedPassword == password)
        {
            // Actualizar último login
            user.LastLoginAt = DateTime.UtcNow;
            return user;
        }

        return null;
    }

    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    public async Task<User?> GetByIdAsync(int userId)
    {
        await Task.Delay(10); // Simular latencia
        return _users.FirstOrDefault(u => u.Id == userId);
    }

    /// <summary>
    /// Obtiene un usuario por su nombre de usuario
    /// </summary>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        await Task.Delay(10); // Simular latencia
        return _users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Obtiene todos los usuarios del sistema
    /// </summary>
    public async Task<List<User>> GetAllAsync()
    {
        await Task.Delay(20); // Simular latencia
        return _users.Where(u => u.IsActive).ToList();
    }

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico
    /// </summary>
    public async Task<bool> HasPermissionAsync(int userId, Permission permission)
    {
        var user = await GetByIdAsync(userId);
        return user?.HasPermission(permission) ?? false;
    }

    /// <summary>
    /// Método auxiliar para obtener credenciales de prueba
    /// </summary>
    public static Dictionary<string, string> GetTestCredentials()
    {
        return new Dictionary<string, string>(_passwords);
    }

    /// <summary>
    /// Método auxiliar para obtener información de usuarios de prueba
    /// </summary>
    public static List<(string Username, string Role, string[] Permissions)> GetTestUsersInfo()
    {
        return _users.Where(u => u.IsActive).Select(u => (
            u.Username,
            string.Join(", ", u.Roles.Select(r => r.ToString())),
            u.GetEffectivePermissions().Select(p => p.ToClaimValue()).ToArray()
        )).ToList();
    }
}
