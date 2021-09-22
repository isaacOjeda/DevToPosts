
namespace MultiTenantSingleDatabase.MultiTenancy;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MultiTenants.Fx;
using MultiTenantSingleDatabase.Persistence;
using System;
using System.Threading.Tasks;

public class DbContextTenantStore : ITenantStore<Tenant>
{
    private readonly TenantAdminDbContext _context;
    private readonly IMemoryCache _cache;

    public DbContextTenantStore(TenantAdminDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Tenant> GetTenantAsync(string identifier)
    {
        var cacheKey = $"Cache_{identifier}";
        var tenant = _cache.Get<Tenant>(cacheKey);

        if (tenant is null)
        {
            var entity = await _context.Tenants
                .FirstOrDefaultAsync(q => q.Identifier == identifier)
                    ?? throw new ArgumentNullException($"identifier no es un tenant válido");

            tenant = new Tenant(entity.TenantId, entity.Identifier);

            tenant.Items["Name"] = entity.Name;
            tenant.Items["ConnectionString"] = entity.ConnectionString;

            _cache.Set(cacheKey, tenant);
        }

        return tenant;
    }
}
