using System.Threading.Channels;

namespace ApiBackgroundChannels.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var notificationGroup = app.MapGroup("api/notification")
            .WithTags("Notification");

        /// <summary>
        /// Envía una notificación
        /// </summary>
        notificationGroup.MapPost("send", async (
            ILogger<Program> logger,
            Channel<NotificationCommand> channel,
            NotificationRequest request) =>
        {
            logger.LogInformation("Solicitud para enviar notificación recibida");

            var tcs = new TaskCompletionSource<NotificationStatus>();
            var command = new NotificationCommand
            {
                Message = request.Message,
                Recipient = request.Recipient,
                ResponseTask = tcs
            };

            // Enviar comando al Channel
            await channel.Writer.WriteAsync(command);

            // Esperar respuesta
            var status = await tcs.Task;
            return Results.Ok(status);
        })
        .WithName("SendNotification");

        return app;
    }
}

public class NotificationRequest
{
    public string Message { get; set; }
    public string Recipient { get; set; }
}

public class NotificationCommand
{
    public string Message { get; set; }
    public string Recipient { get; set; }
    public TaskCompletionSource<NotificationStatus> ResponseTask { get; set; }
}

public class NotificationStatus
{
    public bool Success { get; set; }
    public string Details { get; set; }
}
