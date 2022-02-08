using Microsoft.EntityFrameworkCore;

namespace Basket.Persistence;

public class BasketDbContext : DbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options) : base(options)
    {
        
    }

    public DbSet<Entities.Basket> Baskets => Set<Entities.Basket>();
}