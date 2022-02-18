using Catalog.Persistence;
using MassTransit;
using Messages;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});
//builder.Services.AddMassTransitHostedService();

builder.Services.AddDbContext<CatalogDbContext>(x =>
{
    x.UseInMemoryDatabase(nameof(CatalogDbContext));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("api/products", (CatalogDbContext context) =>
    context.Products
        .AsNoTracking()
        .Select(s => new GetProductsResponse(s.ProductId, s.Description, s.Price))
);

app.MapPut("api/products", async (UpdateProductRequest request, IBus bus, CatalogDbContext context) =>
{
    var product = await context.Products.FindAsync(request.ProductId);

    if (product is null)
    {
        return Results.NotFound();
    }

    var oldPrice = product.Price;

    product.Description = request.Description;
    product.Price = request.Price;

    await context.SaveChangesAsync();

    if (oldPrice != product.Price)
    {
        await bus.Publish(new ProductPriceChanged(product.ProductId, product.Price));
    }

    return Results.Ok();
});

await SeedData();

app.Run();



async Task SeedData()
{
    using var scope = app!.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

    context.Products.Add(new Catalog.Entities.Product
    {
        Description = "Product 01",
        Price = 999
    });

    await context.SaveChangesAsync();
}

record UpdateProductRequest(int ProductId, string Description, double Price);
record GetProductsResponse(int ProductId, string Description, double Price);