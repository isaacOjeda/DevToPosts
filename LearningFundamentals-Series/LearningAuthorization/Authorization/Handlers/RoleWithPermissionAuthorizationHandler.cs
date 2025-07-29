using AdvancedAuthorization.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Handlers;

/// <summary>
/// Handler para validar combinación de roles y permisos
/// </summary>
public class RoleWithPermissionAuthorizationHandler : AuthorizationHandler<RoleWithPermissionRequirement>
{
    /// <summary>
    /// Maneja la validación del requirement
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleWithPermissionRequirement requirement)
    {
        // Verificar roles
        var userRoles = context.User.FindAll("role").Select(c => c.Value).ToHashSet();
        bool hasRequiredRole = requirement.RequiredRoles.Any(role => userRoles.Contains(role));

        // Verificar permisos
        var userPermissions = context.User.FindAll("permissions").Select(c => c.Value).ToHashSet();
        bool hasAllPermissions = requirement.RequiredPermissions.All(permission => userPermissions.Contains(permission));

        // Ambas condiciones deben cumplirse
        if (hasRequiredRole && hasAllPermissions)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
