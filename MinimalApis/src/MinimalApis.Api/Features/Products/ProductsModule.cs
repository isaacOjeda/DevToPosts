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

        app.MapPut("api/products/{productId}", UpdateProduct)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        app.MapDelete("api/products/{productId}", DeleteProduct)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
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

    private static async Task<IResult> UpdateProduct(
        HttpRequest request, MyDbContext context, int productId, Product product)
    {
        var result = request.Validate(product);

        if (!result.IsValid)
        {
            return Results.ValidationProblem(result.ToValidationProblems());
        }

        var exists = await context.Products.AnyAsync(q => q.ProductId == productId);

        if (!exists)
        {
            return Results.Problem(
                detail: $"El producto con ID {productId} no existe",
                statusCode: StatusCodes.Status404NotFound);
        }

        context.Entry(product).State = EntityState.Modified;

        await context.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteProduct(int productId, MyDbContext context)
    {
        var product = await context.Products.FirstOrDefaultAsync(q => q.ProductId == productId);

        if (product is null)
        {
            return Results.Problem(
                detail: $"El producto con ID {productId} no existe",
                statusCode: StatusCodes.Status404NotFound);
        }

        context.Remove(product);

        await context.SaveChangesAsync();

        return Results.NoContent();
    }
}
