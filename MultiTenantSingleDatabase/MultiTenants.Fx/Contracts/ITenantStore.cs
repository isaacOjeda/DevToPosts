
using System.Threading.Tasks;

namespace MultiTenants.Fx;

public interface ITenantStore<T> where T : Tenant
{
    Task<T> GetTenantAsync(string identifier);
}