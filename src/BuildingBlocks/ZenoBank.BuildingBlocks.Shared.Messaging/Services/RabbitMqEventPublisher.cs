using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ZenoBank.BuildingBlocks.Shared.Contracts.Events;
using ZenoBank.BuildingBlocks.Shared.Messaging.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Messaging.Configurations;

namespace ZenoBank.BuildingBlocks.Shared.Messaging.Services;

public class RabbitMqEventPublisher : IEventPublisher
{
    private const string ExchangeName = "zenobank.events";

    private readonly RabbitMqConnection _connection;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    public RabbitMqEventPublisher(
        RabbitMqConnection connection,
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        try
        {
            var connection = _connection.TryGetConnection();

            if (connection is null)
            {
                _logger.LogWarning("RabbitMQ unavailable. Event not published: {EventType}", typeof(TEvent).Name);
                return Task.CompletedTask;
            }

            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null);

            var eventType = typeof(TEvent).Name;
            var json = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Headers = new Dictionary<string, object>
            {
                ["event-type"] = eventType
            };

            channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: eventType,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "RabbitMQ event published. Exchange: {Exchange}, RoutingKey: {RoutingKey}",
                ExchangeName,
                eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish RabbitMQ event: {EventType}", typeof(TEvent).Name);
        }

        return Task.CompletedTask;
    }
}