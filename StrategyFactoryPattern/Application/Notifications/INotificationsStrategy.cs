using StrategyFactoryPattern.Application.Notifications.Models;

namespace StrategyFactoryPattern.Application.Notifications;


public interface INotificationsStrategy
{
    /// <summary>
    /// Mandar una notificación.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<SendNotificationResponse> SendNotification(SendNotificationRequest request);
}


