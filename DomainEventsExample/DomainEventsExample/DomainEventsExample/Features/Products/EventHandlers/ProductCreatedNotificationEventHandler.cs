using DomainEventsExample.Domain.Events;
using DomainEventsExample.Services;
using MediatR;

namespace DomainEventsExample.Features.Products.EventHandlers;

/// <summary>
/// "Notifica" por correo avisando del nuevo producto
/// </summary>
public class ProductCreatedNotificationEventHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedNotificationEventHandler> _logger;
    private readonly IEmailSender _emailSender;

    public ProductCreatedNotificationEventHandler(
        ILogger<ProductCreatedNotificationEventHandler> logger,
        IEmailSender emailSender)
    {
        _logger = logger;
        _emailSender = emailSender;
    }

    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Nueva notificación: Nuevo producto {Product}", notification.Product);

        await _emailSender.SendNotification("random@email.com", "Nuevo Producto", $"Producto {notification.Product.Name}");
    }
}
