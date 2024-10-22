
using StrategyFactoryPattern.Application.Notifications;
using StrategyFactoryPattern.Application.Notifications.Models;

namespace StrategyFactoryPattern.Infrastructure.Notifications.SendGrid;

public class SendGridNotificationService : INotificationsStrategy
{
    private readonly ILogger<SendGridNotificationService> _logger;

    public SendGridNotificationService(ILogger<SendGridNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task<SendNotificationResponse> SendNotification(SendNotificationRequest request)
    {
        _logger.LogInformation("Using SendGrid to send notification");

        // SendGrid logic here
        await Task.Delay(0);

        return new SendNotificationResponse(true, "Notification sent successfully");
    }
}