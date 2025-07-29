using AdvancedAuthorization.Models;

namespace AdvancedAuthorization.Services;

/// <summary>
/// Interfaz para el servicio de generación de tokens JWT
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Genera un token JWT para el usuario especificado
    /// </summary>
    /// <param name="user">Usuario para el cual generar el token</param>
    /// <returns>Token JWT como string</returns>
    string GenerateToken(User user);

    /// <summary>
    /// Valida un token JWT y extrae la información del usuario
    /// </summary>
    /// <param name="token">Token JWT a validar</param>
    /// <returns>ID del usuario si el token es válido, null si no es válido</returns>
    int? ValidateToken(string token);

    /// <summary>
    /// Obtiene el tiempo de expiración configurado para los tokens
    /// </summary>
    /// <returns>Tiempo de expiración en minutos</returns>
    int GetTokenExpiryMinutes();
}
