namespace DomainEventsExample.Services;
public interface IEmailSender
{
    public Task SendNotification(string email, string subject, string body);
}
