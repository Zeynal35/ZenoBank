using System.Text;
using System.Text.Json;
using ZenoBank.BuildingBlocks.Shared.Contracts.Events;
using ZenoBank.BuildingBlocks.Shared.Messaging.Abstractions;

namespace ZenoBank.BuildingBlocks.Shared.Messaging.Services;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly RabbitMqConnection _connection;

    public RabbitMqEventPublisher(RabbitMqConnection connection)
    {
        _connection = connection;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        using var channel = _connection.GetConnection().CreateModel();

        channel.QueueDeclare(
            queue: typeof(TEvent).Name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.Headers = new Dictionary<string, object>
        {
            { "event-type", typeof(TEvent).Name }
        };

        channel.BasicPublish(
            exchange: "",
            routingKey: typeof(TEvent).Name,
            mandatory: false,
            basicProperties: properties,
            body: new ReadOnlyMemory<byte>(body));

        return Task.CompletedTask;
    }
}