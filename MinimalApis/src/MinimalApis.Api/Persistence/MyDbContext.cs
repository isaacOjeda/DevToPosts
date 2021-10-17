using Microsoft.EntityFrameworkCore;
using MinimalApis.Api.Entities;

namespace MinimalApis.Api.Persistence;
public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
    public DbSet<Product> Products => Set<Product>();
}

