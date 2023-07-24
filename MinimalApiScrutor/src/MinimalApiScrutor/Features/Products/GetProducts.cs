namespace MinimalApiScrutor.Features.Products;

public class GetProducts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/products", () => new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Description = "Description 1", Price = 1.99m },
            new Product { Id = 2, Name = "Product 2", Description = "Description 2", Price = 2.99m },
            new Product { Id = 3, Name = "Product 3", Description = "Description 3", Price = 3.99m },
            new Product { Id = 4, Name = "Product 4", Description = "Description 4", Price = 4.99m },
            new Product { Id = 5, Name = "Product 5", Description = "Description 5", Price = 5.99m },
        });
    }
}