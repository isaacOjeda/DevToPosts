using StrategyFactoryPattern.Application.Notifications;
using StrategyFactoryPattern.Application.Notifications.Models;

namespace StrategyFactoryPattern.Infrastructure.Notifications.Smtp;

public class SmtpNotificationsService : INotificationsStrategy
{
    private readonly ILogger<SmtpNotificationsService> _logger;

    public SmtpNotificationsService(ILogger<SmtpNotificationsService> logger)
    {
        _logger = logger;
    }

    public async Task<SendNotificationResponse> SendNotification(SendNotificationRequest request)
    {
        _logger.LogInformation("Using SMTP to send notification");

        // Smtp logic here
        await Task.Delay(0);

        return new SendNotificationResponse(true, "Notification sent successfully");
    }
}