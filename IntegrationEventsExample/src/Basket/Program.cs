using Basket.IntegrationEvents.Consumers;
using Basket.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductPriceChangedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddMassTransitHostedService();

builder.Services.AddDbContext<BasketDbContext>(x =>
{
    x.UseInMemoryDatabase(nameof(BasketDbContext));
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


app.MapGet("/api/basket/{basketId}", (int basketId, BasketDbContext context) =>
{
    return context.Baskets.AsNoTracking()
        .Where(q => q.BasketId == basketId)
        .Select(s => new GetBasketByIdResponse
        {
            BasketId = s.BasketId,
            Products = s.Products
                .Select(s2 => new GetBasketByIdProducts
                {
                    BasketProductId = s2.BasketProductId,
                    ProductDescription = s2.ProductDescription,
                    ProductId = s2.ProductId,
                    Quantity = s2.Quantity,
                    TotalCost = s2.TotalCost,
                    UnitCost = s2.UnitCost
                })
                .ToList()
        })
        .FirstOrDefaultAsync();
});

await SeedData();

app.Run();



async Task SeedData()
{
    using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<BasketDbContext>();

    var newBasket = new Basket.Entities.Basket();
    newBasket.Products.Add(new Basket.Entities.BasketProduct
    {
        ProductDescription = "Product 01",
        Quantity = 1,
        TotalCost = 999,
        UnitCost = 999,
        ProductId = 1
    });

    context.Baskets.Add(newBasket);

    await context.SaveChangesAsync();
}
class GetBasketByIdResponse
{
    public int BasketId { get; set; }
    public List<GetBasketByIdProducts> Products { get; set; } =
        new List<GetBasketByIdProducts>();
}
class GetBasketByIdProducts
{
    public int BasketProductId { get; set; }
    public int ProductId { get; set; }
    public string ProductDescription { get; set; } = default!;
    public int Quantity { get; set; }
    public double UnitCost { get; set; }
    public double TotalCost { get; set; }
}