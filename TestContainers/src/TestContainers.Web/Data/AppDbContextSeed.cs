using TestContainers.Web.Entities;

namespace TestContainers.Web.Data;

public class AppDbContextSeed(AppDbContext context)
{
    public async Task EnsureCreatedAsync()
    {
        await context.Database.EnsureCreatedAsync();
    }
    
    public async Task SeedAsync()
    {
        // Categories
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new() { Name = "Electronics", Description="Electronic devices" },
                new() { Name = "Books", Description="Books" },
                new() { Name = "Clothing", Description="Clothing" },
                new() { Name = "Home & Garden", Description="Home & Garden" },
                new() { Name = "Health & Beauty", Description="Health & Beauty" },
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // Products
        if (!context.Products.Any())
        {
            var electronics = context.Categories.First(c => c.Name == "Electronics");
            var books = context.Categories.First(c => c.Name == "Books");

            var products = new List<Product>
            {
                new() { Name = "Laptop", Price = 1000, Category = electronics },
                new() { Name = "Smartphone", Price = 500, Category = electronics },
                new() { Name = "Tablet", Price = 300, Category = electronics },
                new() { Name = "Headphones", Price = 100, Category = electronics },
                new() { Name = "Book 1", Price = 10, Category = books },
                new() { Name = "Book 2", Price = 15, Category = books},
                new() { Name = "Book 3", Price = 20, Category = books },
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}