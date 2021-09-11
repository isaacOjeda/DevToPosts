
namespace MultiTenantSingleDatabase.Infrastructure.Multitenancy;

public interface ITenantResolutionStrategy
{
    Task<string> GetTenantIdentifierAsync();
}
