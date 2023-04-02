using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;

namespace MultiTenantApi.Data.Tenants;


public class TenantsDbContext : EFCoreStoreDbContext<TenantInfo>
{
    public TenantsDbContext(DbContextOptions options) : base(options)
    {
    }
}