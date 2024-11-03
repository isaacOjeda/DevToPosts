using Microsoft.EntityFrameworkCore;
using TestContainers.Web.Data;
using TestContainers.Web.Entities;

namespace TestContainers.Web.Endpoints;

public static class ProductsEndpoints
{
    public static void MapProductsEndpoints(this WebApplication app)
    {
        app.MapGet("/products", async (AppDbContext context) =>
        {
            var products = await context.Products
                .Include(p => p.Category)
                .ToListAsync();

            return Results.Ok(products);
        });

        app.MapGet("/products/{id}", async (AppDbContext context, int id) =>
        {
            var product = await context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(product);
        });

        app.MapPost("/products", async (AppDbContext context, Product product) =>
        {
            try
            {
                await context.Products.AddAsync(product);
                await context.SaveChangesAsync();
                return Results.Created($"/products/{product.Id}", product);
            }
            catch (DbUpdateException)
            {
                return Results.BadRequest("An error occurred while saving the product.");
            }
        });

        app.MapPut("/products/{id}", async (AppDbContext context, int id, Product product) =>
        {
            if (id != product.Id)
            {
                return Results.BadRequest();
            }

            context.Products.Update(product);
            await context.SaveChangesAsync();

            return Results.Ok(product);
        });

        app.MapDelete("/products/{id}", async (AppDbContext context, int id) =>
        {
            var product = await context.Products.FindAsync(id);

            if (product is null)
            {
                return Results.NotFound();
            }

            context.Products.Remove(product);
            await context.SaveChangesAsync();

            return Results.NoContent();
        });
    }

}