using ApiKeyCustomAuth.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiKeyCustomAuth.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    { }

    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
}