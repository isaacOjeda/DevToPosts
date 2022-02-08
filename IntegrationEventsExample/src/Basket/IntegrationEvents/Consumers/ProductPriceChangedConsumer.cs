using Basket.Persistence;
using MassTransit;
using Messages;
using Microsoft.EntityFrameworkCore;

namespace Basket.IntegrationEvents.Consumers;

public class ProductPriceChangedConsumer : IConsumer<ProductPriceChanged>
{
    private readonly ILogger<ProductPriceChangedConsumer> _log;
    private readonly BasketDbContext _context;

    public ProductPriceChangedConsumer(ILogger<ProductPriceChangedConsumer> log, BasketDbContext context)
    {
        _log = log;
        _context = context;
    }
    public async Task Consume(ConsumeContext<ProductPriceChanged> context)
    {
        _log.LogInformation("Nuevo evento: Precio actualizado del producto {0}.", context.Message.ProductId);
        _log.LogWarning("Price: {0}", context.Message.NewPrice);

        // Actualizar todas las canastas de compra que tienen ese producto
        var baskets = await _context.Baskets
            .Include(i => i.Products)
            .Where(q => q.Products.Any(a => a.ProductId == context.Message.ProductId))
            .ToListAsync();

        foreach (var basket in baskets)
        {
            var productToUpdate = basket.Products
                .FirstOrDefault(q => q.ProductId == context.Message.ProductId);

            if (productToUpdate is null)
            {
                // TODO: Error?
                continue;
            }

            productToUpdate.UnitCost = context.Message.NewPrice;
            productToUpdate.TotalCost = productToUpdate.UnitCost * productToUpdate.Quantity;

            _log.LogWarning("Precio actualizado en la canasta");
        }

        await _context.SaveChangesAsync();
    }
}
