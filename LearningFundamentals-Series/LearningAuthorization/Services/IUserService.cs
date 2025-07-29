using AdvancedAuthorization.Models;

namespace AdvancedAuthorization.Services;

/// <summary>
/// Interfaz para el servicio de gestión de usuarios
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Autentica un usuario con credenciales
    /// </summary>
    /// <param name="username">Nombre de usuario o email</param>
    /// <param name="password">Contraseña</param>
    /// <returns>Usuario autenticado o null si las credenciales son inválidas</returns>
    Task<User?> AuthenticateAsync(string username, string password);

    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>Usuario encontrado o null</returns>
    Task<User?> GetByIdAsync(int userId);

    /// <summary>
    /// Obtiene un usuario por su nombre de usuario
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <returns>Usuario encontrado o null</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Obtiene todos los usuarios del sistema
    /// </summary>
    /// <returns>Lista de usuarios</returns>
    Task<List<User>> GetAllAsync();

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="permission">Permiso a verificar</param>
    /// <returns>True si tiene el permiso, false en caso contrario</returns>
    Task<bool> HasPermissionAsync(int userId, Permission permission);
}
