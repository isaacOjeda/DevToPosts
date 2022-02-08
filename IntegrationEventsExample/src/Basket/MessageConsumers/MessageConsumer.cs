using MassTransit;
using Messages;

namespace Basket.MessageConsumers;

public class MessageConsumer : IConsumer<Message>
{
    readonly ILogger<MessageConsumer> _logger;

    public MessageConsumer(ILogger<MessageConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Message> context)
    {
        _logger.LogInformation("Received Text: {Text}", context.Message.Text);

        return Task.CompletedTask;
    }
}
