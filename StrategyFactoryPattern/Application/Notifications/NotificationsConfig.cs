namespace StrategyFactoryPattern.Application.Notifications;

public class NotificationsConfig
{
    public NotificationProvider Provider { get; set; }

    public SendGridConfig? SendGrid { get; set; }
    public SmtpConfig? Smtp { get; set; }

}

public class SendGridConfig
{
    public string ApiKey { get; set; } = string.Empty;
}

public class SmtpConfig
{
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}