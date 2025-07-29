using AdvancedAuthorization.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace LearningAuthorization.Authorization.Policies;

public static class AuthorizationPolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequirePermissions(this AuthorizationPolicyBuilder builder, params string[] requiredPermissions)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (requiredPermissions == null || !requiredPermissions.Any())
        {
            throw new ArgumentException("Required permissions cannot be null or empty.", nameof(requiredPermissions));
        }

        builder.AddRequirements(new PermissionsRequirement(requiredPermissions));

        return builder;
    }


    public static AuthorizationPolicyBuilder RequireAnyPermissions(this AuthorizationPolicyBuilder builder, params string[] requiredPermissions)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (requiredPermissions == null || !requiredPermissions.Any())
        {
            throw new ArgumentException("Required permissions cannot be null or empty.", nameof(requiredPermissions));
        }

        builder.AddRequirements(new AnyPermissionRequirement(requiredPermissions));

        return builder;
    }
}
