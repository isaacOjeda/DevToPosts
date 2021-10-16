using Carter;
using Carter.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MinimalApis.Api.Entities;
using MinimalApis.Api.Extensions;
using MinimalApis.Api.Persistence;

namespace MinimalApis.Api.Features.Products;
public class ProductsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products", GetProducts)
            .Produces<List<Product>>();

        app.MapGet("api/products/{productId}", GetProduct)
            .Produces<Product>()
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("api/products", CreateProduct)
            .Produces<Product>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> GetProducts(MyDbContext context)
    {
        var products = await context.Products
            .ToListAsync();

        return Results.Ok(products);
    }

    private static async Task<IResult> GetProduct(int productId, MyDbContext context)
    {
        var product = await context.Products.FindAsync(productId);

        if (product is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(product);
    }

    private static async Task<IResult> CreateProduct(HttpRequest req, Product product, MyDbContext context)
    {
        var result = req.Validate(product);

        if (!result.IsValid)
        {
            return Results.ValidationProblem(result.ToValidationProblems());
        }

        context.Products.Add(product);

        await context.SaveChangesAsync();

        return Results.Created($"api/products/{product.ProductId}", product);
    }
}
