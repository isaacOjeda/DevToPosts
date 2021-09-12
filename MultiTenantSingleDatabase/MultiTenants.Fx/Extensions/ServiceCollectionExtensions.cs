
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenants.Fx;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Agrega los servicios (con clase específica)
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static TenantBuilder<T> AddMultiTenancy<T>(this IServiceCollection services) where T : Tenant
        => new(services);

    /// <summary>
    /// Agrega los servicios (con clase default)
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static TenantBuilder<Tenant> AddMultiTenancy(this IServiceCollection services)
        => new(services);
}
