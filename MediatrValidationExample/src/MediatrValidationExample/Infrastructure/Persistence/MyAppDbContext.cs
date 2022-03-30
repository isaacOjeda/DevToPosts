using MediatrValidationExample.Domain;
using Microsoft.EntityFrameworkCore;

namespace MediatrValidationExample.Infrastructure.Persistence;
public class MyAppDbContext : DbContext
{
    public MyAppDbContext(DbContextOptions<MyAppDbContext> options) : base(options)
    {

    }

    public DbSet<Product> Products => Set<Product>();
}
