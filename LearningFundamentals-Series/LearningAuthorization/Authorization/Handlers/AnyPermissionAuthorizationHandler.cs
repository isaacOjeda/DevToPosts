using AdvancedAuthorization.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Handlers;

/// <summary>
/// Handler para validar permisos alternativos (cualquiera válido)
/// </summary>
public class AnyPermissionAuthorizationHandler : AuthorizationHandler<AnyPermissionRequirement>
{
    /// <summary>
    /// Maneja la validación del requirement
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyPermissionRequirement requirement)
    {
        // Obtener todos los permisos del usuario
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToHashSet();

        // Verificar que el usuario tenga AL MENOS UNO de los permisos requeridos
        bool hasAnyPermission = requirement.Permissions.Any(permission => userPermissions.Contains(permission));

        if (hasAnyPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
