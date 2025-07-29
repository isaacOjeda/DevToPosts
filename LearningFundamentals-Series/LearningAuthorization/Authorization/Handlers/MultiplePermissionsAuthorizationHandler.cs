using AdvancedAuthorization.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Handlers;

/// <summary>
/// Handler para validar múltiples permisos (todos requeridos)
/// </summary>
public class MultiplePermissionsAuthorizationHandler : AuthorizationHandler<MultiplePermissionsRequirement>
{
    /// <summary>
    /// Maneja la validación del requirement
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MultiplePermissionsRequirement requirement)
    {
        // Obtener todos los permisos del usuario
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToHashSet();

        // Verificar que el usuario tenga TODOS los permisos requeridos
        bool hasAllPermissions = requirement.Permissions.All(permission => userPermissions.Contains(permission));

        if (hasAllPermissions)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
