namespace AdvancedAuthorization.Models;

/// <summary>
/// DTO para solicitudes de login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Nombre de usuario o email
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    public required string Password { get; set; }
}

/// <summary>
/// DTO para respuestas de autenticación exitosa
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Token JWT generado
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Tipo de token (Bearer)
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Tiempo de expiración en segundos
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Información del usuario autenticado
    /// </summary>
    public required UserInfo User { get; set; }
}

/// <summary>
/// DTO con información básica del usuario
/// </summary>
public class UserInfo
{
    /// <summary>
    /// ID del usuario
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de usuario
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Nombre completo
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// Email del usuario
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Roles del usuario
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Permisos efectivos del usuario
    /// </summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Departamento del usuario
    /// </summary>
    public string? Department { get; set; }
}

/// <summary>
/// Respuesta estándar para endpoints de la API
/// </summary>
/// <typeparam name="T">Tipo de datos en la respuesta</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje descriptivo
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Datos de la respuesta
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Lista de errores si los hay
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Timestamp de la respuesta
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Respuesta simplificada para casos sin datos específicos
/// </summary>
public class ApiResponse : ApiResponse<object>
{
}
