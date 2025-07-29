using AdvancedAuthorization.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Handlers;

/// <summary>
/// Handler para validar departamento del usuario
/// </summary>
public class DepartmentAuthorizationHandler : AuthorizationHandler<DepartmentRequirement>
{
    /// <summary>
    /// Maneja la validación del requirement
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DepartmentRequirement requirement)
    {
        // Obtener el departamento del usuario
        var userDepartment = context.User.FindFirst("department")?.Value;

        // Si no tiene departamento
        if (string.IsNullOrEmpty(userDepartment))
        {
            if (requirement.AllowNoDepartment)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail(new AuthorizationFailureReason(
                    this,
                    "Usuario no tiene departamento asignado y se requiere uno de los siguientes: " +
                    string.Join(", ", requirement.AllowedDepartments)));
            }
            return Task.CompletedTask;
        }

        // Verificar si el departamento está permitido
        if (requirement.AllowedDepartments.Contains(userDepartment, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(
                this,
                $"Departamento '{userDepartment}' no está autorizado. Departamentos permitidos: {string.Join(", ", requirement.AllowedDepartments)}"));
        }

        return Task.CompletedTask;
    }
}
