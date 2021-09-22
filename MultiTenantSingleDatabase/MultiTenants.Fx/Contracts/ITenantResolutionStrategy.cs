
using System.Threading.Tasks;

namespace MultiTenants.Fx;

public interface ITenantResolutionStrategy
{
    Task<string> GetTenantIdentifierAsync();
}
