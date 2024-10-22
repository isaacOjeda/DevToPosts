using StrategyFactoryPattern.Application.Notifications;
using StrategyFactoryPattern.Application.Notifications.Models;

namespace StrategyFactoryPattern.Infrastructure.Notifications;


public class NotificationService(INotificationsFactory notificationsFactory) : INotificationsStrategy
{
    private INotificationsStrategy notificationsStrategy = notificationsFactory.CreateNotificationService();

    public async Task<SendNotificationResponse> SendNotification(SendNotificationRequest request)
    {
        return await notificationsStrategy.SendNotification(request);
    }
}