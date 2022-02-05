namespace DomainEventsExample.Services;
public class FakeEmailSender : IEmailSender
{
    private readonly ILogger<FakeEmailSender> _logger;

    public FakeEmailSender(ILogger<FakeEmailSender> logger)
    {
        _logger = logger;
    }

    public async Task SendNotification(string email, string subject, string body)
    {
        _logger.LogInformation("Mandando correo a {Email} ", email);
        _logger.LogInformation("Body: {Body}", body);

        await Task.Delay(1000);
    }
}
