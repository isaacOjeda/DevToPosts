using AdvancedAuthorization.Models;
using AdvancedAuthorization.Models.Constants;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AdvancedAuthorization.Services;

/// <summary>
/// Servicio mock para generación y validación de tokens JWT
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        _issuer = _configuration["Jwt:Issuer"] ?? AuthConstants.JwtDefaults.DefaultIssuer;
        _audience = _configuration["Jwt:Audience"] ?? AuthConstants.JwtDefaults.DefaultAudience;
        _expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryInMinutes", AuthConstants.JwtDefaults.DefaultExpiryMinutes);
    }

    /// <summary>
    /// Genera un token JWT para el usuario especificado
    /// </summary>
    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_key);

        var claims = BuildClaims(user);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Valida un token JWT y extrae la información del usuario
    /// </summary>
    public int? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == AuthConstants.ClaimTypes.UserId);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Obtiene el tiempo de expiración configurado para los tokens
    /// </summary>
    public int GetTokenExpiryMinutes()
    {
        return _expiryMinutes;
    }

    /// <summary>
    /// Construye la lista de claims para el usuario
    /// </summary>
    private static List<Claim> BuildClaims(User user)
    {
        var claims = new List<Claim>
        {
            // Claims estándar
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            
            // Claims personalizados
            new(AuthConstants.ClaimTypes.UserId, user.Id.ToString()),
            new(AuthConstants.ClaimTypes.FullName, user.FullName),
            new(AuthConstants.ClaimTypes.IsActive, user.IsActive.ToString())
        };

        // Agregar roles como claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToClaimValue()));
        }

        // Agregar departamento si existe
        if (!string.IsNullOrEmpty(user.Department))
        {
            claims.Add(new Claim(AuthConstants.ClaimTypes.Department, user.Department));
        }

        // Agregar permisos efectivos como claims
        var effectivePermissions = user.GetEffectivePermissions();
        foreach (var permission in effectivePermissions)
        {
            claims.Add(new Claim(AuthConstants.ClaimTypes.Permissions, permission.ToClaimValue()));
        }

        return claims;
    }
}
