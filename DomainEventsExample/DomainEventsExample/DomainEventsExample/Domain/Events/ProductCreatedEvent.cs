using DomainEventsExample.Domain.Entities;

namespace DomainEventsExample.Domain.Events;
public class ProductCreatedEvent : DomainEvent
{
    public ProductCreatedEvent(Product product)
    {
        Product = product;
    }

    public Product Product { get; }
}
