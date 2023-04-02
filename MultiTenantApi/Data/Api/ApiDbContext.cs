using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using MultiTenantApi.Entities;

namespace MultiTenantApi.Data.Api;

public class ApiDbContext : DbContext
{
    private readonly ITenantInfo? _tenant;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public ApiDbContext(
        DbContextOptions<ApiDbContext> options,
        IWebHostEnvironment env,
        IMultiTenantContextAccessor multiTenantContextAccessor,
        IConfiguration config)
        : base(options)
    {
        _tenant = multiTenantContextAccessor.MultiTenantContext?.TenantInfo;
        _env = env;
        _config = config;
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string? connectionString;

        if (_tenant is null && _env.IsDevelopment())
        {
            // Init/Dev connection string
            connectionString = _config.GetConnectionString("Default");
        }
        else
        {
            // Tenant connection string
            connectionString = _tenant!.ConnectionString;
        }

        optionsBuilder.UseSqlServer(connectionString);

        base.OnConfiguring(optionsBuilder);
    }
}