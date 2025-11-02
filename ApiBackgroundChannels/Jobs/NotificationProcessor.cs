using System.Threading.Channels;
using ApiBackgroundChannels.Endpoints;

namespace ApiBackgroundChannels.Jobs;

public class NotificationProcessor : BackgroundService
{
    private readonly ILogger<NotificationProcessor> _logger;
    private readonly Channel<NotificationCommand> _channel;

    public NotificationProcessor(
        ILogger<NotificationProcessor> logger,
        Channel<NotificationCommand> channel)
    {
        _logger = logger;
        _channel = channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationProcessor iniciado. Esperando comandos...");

        while (await _channel.Reader.WaitToReadAsync(stoppingToken))
        {
            while (_channel.Reader.TryRead(out var command))
            {
                try
                {
                    // Simular envío de notificación
                    _logger.LogInformation("Enviando notificación a {Recipient}: {Message}",
                        command.Recipient, command.Message);

                    // Notificar éxito
                    command.ResponseTask?.SetResult(new NotificationStatus
                    {
                        Success = true,
                        Details = "Notificación programada exitosamente."
                    });

                    var randomTime = new Random().Next(500, 2000);
                    await Task.Delay(randomTime, stoppingToken); // Simular tiempo de envío

                    _logger.LogInformation("Notificación enviada a {Recipient}", command.Recipient);
                    _logger.LogInformation("Timestamp: {Timestamp}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando notificación");

                    // Notificar error al productor
                    command.ResponseTask?.SetResult(new NotificationStatus
                    {
                        Success = false,
                        Details = $"Error: {ex.Message}"
                    });
                }
            }
        }
    }
}