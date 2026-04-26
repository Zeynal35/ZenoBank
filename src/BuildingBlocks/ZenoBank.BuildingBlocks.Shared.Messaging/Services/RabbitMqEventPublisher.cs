using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZenoBank.BuildingBlocks.Shared.Contracts.Events;
using ZenoBank.BuildingBlocks.Shared.Messaging.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Messaging.Configurations;

namespace ZenoBank.BuildingBlocks.Shared.Messaging.Services;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly RabbitMqConnection _connection;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    public RabbitMqEventPublisher(
        RabbitMqConnection connection,
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _connection = connection;
        _settings = settings.Value;
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        try
        {
            // ✅ RabbitMQ yoxdursa, xəta vermirik - sadəcə log yazırıq
            var conn = _connection.TryGetConnection();
            if (conn is null)
            {
                _logger.LogWarning("RabbitMQ unavailable. Skipping event: {EventType}", typeof(TEvent).Name);
                return Task.CompletedTask;
            }

            using var channel = conn.CreateModel();

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

            _logger.LogInformation("Event published: {EventType}", typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            // ✅ RabbitMQ xətası login/register-i bloklamamalıdır
            _logger.LogWarning("Failed to publish event {EventType}: {Message}", typeof(TEvent).Name, ex.Message);
        }

        return Task.CompletedTask;
    }
}
