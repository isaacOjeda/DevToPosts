using AdvancedAuthorization.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AdvancedAuthorization.Authorization.Services;

/// <summary>
/// Implementación del builder de políticas
/// </summary>
public class PolicyBuilder : IPolicyBuilder
{
    private readonly AuthorizationPolicyBuilder _policyBuilder;

    /// <summary>
    /// Constructor
    /// </summary>
    public PolicyBuilder()
    {
        _policyBuilder = new AuthorizationPolicyBuilder();
        _policyBuilder.RequireAuthenticatedUser(); // Siempre requerir autenticación
    }

    /// <summary>
    /// Requiere un permiso específico
    /// </summary>
    public IPolicyBuilder RequirePermission(string permission)
    {
        _policyBuilder.AddRequirements(new PermissionRequirement(permission));
        return this;
    }

    /// <summary>
    /// Requiere múltiples permisos (todos)
    /// </summary>
    public IPolicyBuilder RequireAllPermissions(params string[] permissions)
    {
        _policyBuilder.AddRequirements(new MultiplePermissionsRequirement(permissions));
        return this;
    }

    /// <summary>
    /// Requiere cualquiera de los permisos especificados
    /// </summary>
    public IPolicyBuilder RequireAnyPermission(params string[] permissions)
    {
        _policyBuilder.AddRequirements(new AnyPermissionRequirement(permissions));
        return this;
    }

    /// <summary>
    /// Requiere un rol específico con permisos
    /// </summary>
    public IPolicyBuilder RequireRoleWithPermissions(string[] roles, string[] permissions)
    {
        _policyBuilder.AddRequirements(new RoleWithPermissionRequirement(roles, permissions));
        return this;
    }

    /// <summary>
    /// Requiere horario laboral
    /// </summary>
    public IPolicyBuilder RequireWorkingHours(TimeSpan startTime, TimeSpan endTime, bool allowAdminBypass = true)
    {
        _policyBuilder.AddRequirements(new WorkingHoursRequirement(startTime, endTime, allowAdminBypass: allowAdminBypass));
        return this;
    }

    /// <summary>
    /// Requiere departamento específico
    /// </summary>
    public IPolicyBuilder RequireDepartment(params string[] departments)
    {
        _policyBuilder.AddRequirements(new DepartmentRequirement(departments));
        return this;
    }

    /// <summary>
    /// Requiere acceso condicional
    /// </summary>
    public IPolicyBuilder RequireConditionalAccess(Action<ConditionalAccessConditions> configure)
    {
        var conditions = new ConditionalAccessConditions();
        configure(conditions);
        _policyBuilder.AddRequirements(new ConditionalAccessRequirement(conditions));
        return this;
    }

    /// <summary>
    /// Construye la política de autorización
    /// </summary>
    public AuthorizationPolicy Build()
    {
        return _policyBuilder.Build();
    }
}

/// <summary>
/// Extensiones para facilitar la construcción de políticas
/// </summary>
public static class PolicyBuilderExtensions
{
    /// <summary>
    /// Inicia la construcción de una nueva política
    /// </summary>
    public static IPolicyBuilder NewPolicy()
    {
        return new PolicyBuilder();
    }

    /// <summary>
    /// Política para acceso básico con un permiso
    /// </summary>
    public static AuthorizationPolicy BasicPermissionPolicy(string permission)
    {
        return NewPolicy()
            .RequirePermission(permission)
            .Build();
    }

    /// <summary>
    /// Política para acceso administrativo
    /// </summary>
    public static AuthorizationPolicy AdminPolicy()
    {
        return NewPolicy()
            .RequireRoleWithPermissions(
                roles: new[] { "Admin", "SuperAdmin" },
                permissions: new[] { "adminaccess" })
            .Build();
    }

    /// <summary>
    /// Política para operaciones críticas
    /// </summary>
    public static AuthorizationPolicy CriticalOperationPolicy()
    {
        return NewPolicy()
            .RequireAllPermissions("adminaccess", "systemconfigwrite")
            .RequireWorkingHours(new TimeSpan(8, 0, 0), new TimeSpan(18, 0, 0))
            .Build();
    }

    /// <summary>
    /// Política para acceso departamental
    /// </summary>
    public static AuthorizationPolicy DepartmentPolicy(params string[] departments)
    {
        return NewPolicy()
            .RequireDepartment(departments)
            .RequireAnyPermission("usersread", "reportsread", "adminaccess")
            .Build();
    }

    /// <summary>
    /// Política para acceso condicional avanzado
    /// </summary>
    public static AuthorizationPolicy AdvancedConditionalPolicy()
    {
        return NewPolicy()
            .RequireConditionalAccess(conditions =>
            {
                conditions.RequireMfa = true;
                conditions.MaxTokenAgeMinutes = 60;
                conditions.RequireWorkingHours = true;
                conditions.MinimumSecurityLevel = "Medium";
            })
            .Build();
    }

    /// <summary>
    /// Política para gestión de usuarios con múltiples validaciones
    /// </summary>
    public static AuthorizationPolicy UserManagementPolicy()
    {
        return NewPolicy()
            .RequireRoleWithPermissions(
                roles: new[] { "Manager", "Admin", "SuperAdmin" },
                permissions: new[] { "usersread" })
            .RequireDepartment("IT", "HR", "Management")
            .RequireWorkingHours(new TimeSpan(7, 0, 0), new TimeSpan(19, 0, 0))
            .Build();
    }

    /// <summary>
    /// Política para reportes financieros con alta seguridad
    /// </summary>
    public static AuthorizationPolicy FinancialReportsPolicy()
    {
        return NewPolicy()
            .RequireAllPermissions("reportsread", "adminaccess")
            .RequireDepartment("Finance", "Accounting", "Executive")
            .RequireConditionalAccess(conditions =>
            {
                conditions.RequireMfa = true;
                conditions.MaxTokenAgeMinutes = 30;
                conditions.MinimumSecurityLevel = "High";
            })
            .Build();
    }
}
