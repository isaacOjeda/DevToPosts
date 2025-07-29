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
