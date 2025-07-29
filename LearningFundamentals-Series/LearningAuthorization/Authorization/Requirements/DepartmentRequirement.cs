using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Requirements;

/// <summary>
/// Requirement que valida el departamento del usuario
/// </summary>
public class DepartmentRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Los departamentos permitidos
    /// </summary>
    public IReadOnlyList<string> AllowedDepartments { get; }

    /// <summary>
    /// Si se permite acceso a usuarios sin departamento asignado
    /// </summary>
    public bool AllowNoDepartment { get; }

    /// <summary>
    /// Constructor del requirement
    /// </summary>
    /// <param name="allowedDepartments">Departamentos permitidos</param>
    /// <param name="allowNoDepartment">Permitir usuarios sin departamento</param>
    public DepartmentRequirement(string[] allowedDepartments, bool allowNoDepartment = false)
    {
        if (allowedDepartments == null || allowedDepartments.Length == 0)
            throw new ArgumentException("Se requiere al menos un departamento", nameof(allowedDepartments));

        AllowedDepartments = allowedDepartments.ToList().AsReadOnly();
        AllowNoDepartment = allowNoDepartment;
    }

    /// <summary>
    /// Descripción del requirement para debugging
    /// </summary>
    public override string ToString()
    {
        var depts = string.Join(", ", AllowedDepartments);
        var noDept = AllowNoDepartment ? " (permite sin departamento)" : "";
        return $"DepartmentRequirement: {depts}{noDept}";
    }
}
