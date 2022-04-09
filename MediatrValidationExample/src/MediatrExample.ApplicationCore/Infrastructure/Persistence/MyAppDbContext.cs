using MediatrExample.ApplicationCore.Common.Interfaces;
using MediatrExample.ApplicationCore.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediatrExample.ApplicationCore.Infrastructure.Persistence;
public class MyAppDbContext : IdentityDbContext<IdentityUser>
{
    private readonly CurrentUser _user;

    public MyAppDbContext(
        DbContextOptions<MyAppDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _user = currentUserService.User;
    }

    public DbSet<Product> Products => Set<Product>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _user.Id;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = _user.Id;
                    entry.Entity.LastModifiedByAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
