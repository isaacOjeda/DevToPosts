
using Microsoft.EntityFrameworkCore;
using MultiTenants.Fx.Contracts;
using MultiTenantSingleDatabase.Models;

namespace MultiTenantSingleDatabase.Persistence;

public class SingleTenantDbContext : DbContext
{
    private readonly MultiTenants.Fx.Tenant _tenant;

    public SingleTenantDbContext(
        DbContextOptions<SingleTenantDbContext> options,
        ITenantAccessor<MultiTenants.Fx.Tenant> tenantAccessor) : base(options)
    {
        _tenant = tenantAccessor.Tenant ?? throw new ArgumentNullException(nameof(MultiTenants.Fx.Tenant));
    }

    public DbSet<Product> Products { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSqlServer(_tenant.Items["ConnectionString"]?.ToString());
    }
}
