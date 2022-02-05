using DomainEventsExample.Domain.Events;

namespace DomainEventsExample.Domain.Entities;
public class Product : IHasDomainEvent
{
    public Product(int productId, string name, string description, double price)
    {
        ProductId = productId;
        Name = name;
        Description = description;
        Price = price;

        DomainEvents.Add(new ProductCreatedEvent(this));
    }

    public int ProductId { get; set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public double Price { get; private set; }

    public List<DomainEvent> DomainEvents { get; set; } = new List<DomainEvent>();
}