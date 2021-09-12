
using Microsoft.AspNetCore.Http;

namespace MultiTenants.Fx;


/// <summary>
/// Extensiones de HttpContext para hacer multi-tenancy más fácil de usar
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Regresa el Tenant actual
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    public static T? GetTenant<T>(this HttpContext context) where T : Tenant
    {
        if (!context.Items.ContainsKey(AppConstants.HttpContextTenantKey))
            return null;

        return context.Items[AppConstants.HttpContextTenantKey] as T;
    }

    /// <summary>
    /// Regresa el Tenant actual
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static Tenant? GetTenant(this HttpContext context) => context.GetTenant<Tenant>();
}