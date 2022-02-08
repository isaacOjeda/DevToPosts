using Catalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Persistence;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
        
    }

    public DbSet<Product> Products => Set<Product>();
}
