using Microsoft.Extensions.Options;
using StrategyFactoryPattern.Application.Notifications;
using StrategyFactoryPattern.Infrastructure.Notifications.SendGrid;
using StrategyFactoryPattern.Infrastructure.Notifications.Smtp;

namespace StrategyFactoryPattern.Infrastructure.Notifications;

public class NotificationsFactory(
    ILogger<NotificationsFactory> logger,
    IServiceProvider serviceProvider,
    IOptions<NotificationsConfig> notificationsConfig) : INotificationsFactory
{
    public INotificationsStrategy CreateNotificationService()
    {
        var provider = notificationsConfig.Value.Provider;

        logger.LogInformation("Creating notification service for provider {Provider}", provider);

        return provider switch
        {
            NotificationProvider.SendGrid => serviceProvider.GetRequiredService<SendGridNotificationService>(),
            NotificationProvider.Smtp => serviceProvider.GetRequiredService<SmtpNotificationsService>(),
            _ => throw new NotImplementedException($"Notification provider {provider} is not implemented")
        };
    }

    public INotificationsStrategy CreateNotificationService(NotificationProvider provider)
    {
        logger.LogInformation("Creating notification service for provider {Provider}", provider);

        return provider switch
        {
            NotificationProvider.SendGrid => serviceProvider.GetRequiredService<SendGridNotificationService>(),
            NotificationProvider.Smtp => serviceProvider.GetRequiredService<SmtpNotificationsService>(),
            _ => throw new NotImplementedException($"Notification provider {provider} is not implemented")
        };
    }
}