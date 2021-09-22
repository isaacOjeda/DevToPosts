
namespace MultiTenantSingleDatabase.Persistence;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiTenants.Fx;
using MultiTenantSingleDatabase.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

public class MultiTenantDbContext : DbContext
{
    private readonly int _tenantId;

    public MultiTenantDbContext(
        DbContextOptions<MultiTenantDbContext> options,
        IHttpContextAccessor contextAccessor) : base(options)
    {
        var currentTenant = contextAccessor.HttpContext?.GetTenant();
        _tenantId = currentTenant?.Id ?? 0;

        this.Filter<AuditableEntity>(f => f.Where(q => q.TenantId == _tenantId));
    }

    public DbSet<Product> Products { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.TenantId = _tenantId;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
