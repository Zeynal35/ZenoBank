using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ZenoBank.BuildingBlocks.Shared.Contracts.Events;
using ZenoBank.BuildingBlocks.Shared.Messaging.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Messaging.Configurations;

namespace ZenoBank.BuildingBlocks.Shared.Messaging.Services;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly RabbitMqConnection _connection;
    private readonly RabbitMqSettings _settings;

    public RabbitMqEventPublisher(RabbitMqConnection connection, IOptions<RabbitMqSettings> settings)
    {
        _connection = connection;
        _settings = settings.Value;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        using var channel = _connection.GetConnection().CreateModel();

        channel.QueueDeclare(
            queue: _settings.QueueName,
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
            routingKey: _settings.QueueName,
            mandatory: false,
            basicProperties: properties,
            body: new ReadOnlyMemory<byte>(body));

        return Task.CompletedTask;
    }
}