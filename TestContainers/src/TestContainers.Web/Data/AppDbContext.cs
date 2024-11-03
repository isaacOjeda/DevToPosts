using Microsoft.EntityFrameworkCore;
using TestContainers.Web.Entities;

namespace TestContainers.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<Product>();

        product.HasKey(p => p.Id);
        product.Property(p => p.Name).IsRequired();
        product.Property(p => p.Description).IsRequired();
        product.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        product.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId);

        var category = modelBuilder.Entity<Category>();

        category.HasKey(c => c.Id);
        category.Property(c => c.Name).IsRequired();
        category.Property(c => c.Description).IsRequired();

    }

}