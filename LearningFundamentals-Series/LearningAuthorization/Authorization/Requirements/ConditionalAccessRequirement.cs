using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Requirements;

/// <summary>
/// Requirement que valida múltiples condiciones con lógica compleja
/// </summary>
public class ConditionalAccessRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Condiciones que deben cumplirse
    /// </summary>
    public ConditionalAccessConditions Conditions { get; }

    /// <summary>
    /// Constructor del requirement
    /// </summary>
    /// <param name="conditions">Las condiciones a evaluar</param>
    public ConditionalAccessRequirement(ConditionalAccessConditions conditions)
    {
        Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
    }

    /// <summary>
    /// Descripción del requirement para debugging
    /// </summary>
    public override string ToString()
    {
        return $"ConditionalAccessRequirement: {Conditions}";
    }
}


/// <summary>
/// Condiciones para acceso condicional
/// </summary>
public class ConditionalAccessConditions
{
    /// <summary>
    /// Requiere MFA (Multi-Factor Authentication)
    /// </summary>
    public bool RequireMfa { get; set; }

    /// <summary>
    /// Edad máxima del token en minutos
    /// </summary>
    public int? MaxTokenAgeMinutes { get; set; }

    /// <summary>
    /// Horario laboral requerido
    /// </summary>
    public bool RequireWorkingHours { get; set; }

    /// <summary>
    /// Departamentos permitidos
    /// </summary>
    public string[]? AllowedDepartments { get; set; }

    /// <summary>
    /// Nivel mínimo de seguridad requerido
    /// </summary>
    public string? MinimumSecurityLevel { get; set; }

    /// <summary>
    /// Descripción de las condiciones
    /// </summary>
    public override string ToString()
    {
        var conditions = new List<string>();

        if (RequireMfa) conditions.Add("MFA requerido");
        if (MaxTokenAgeMinutes.HasValue) conditions.Add($"Token < {MaxTokenAgeMinutes}min");
        if (RequireWorkingHours) conditions.Add("Horario laboral");
        if (AllowedDepartments?.Length > 0) conditions.Add($"Depts: {string.Join(",", AllowedDepartments)}");
        if (!string.IsNullOrEmpty(MinimumSecurityLevel)) conditions.Add($"Security: {MinimumSecurityLevel}");

        return string.Join(" AND ", conditions);
    }
}
