using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using ZenoBank.BuildingBlocks.Shared.Contracts.Events;
using ZenoBank.BuildingBlocks.Shared.Messaging.Configurations;
using ZenoBank.Services.Notification.Domain.Entities;
using ZenoBank.Services.Notification.Domain.Enums;
using ZenoBank.Services.Notification.Infrastructure.Persistence;

namespace ZenoBank.Services.Notification.Infrastructure.BackgroundWorkers;

public class TransactionEventsConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IModel? _channel;

    public TransactionEventsConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> settings)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.BasicQos(0, 1, false);

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var eventType = GetEventType(ea.BasicProperties);

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

                switch (eventType)
                {
                    case nameof(DepositCompletedEvent):
                        {
                            var @event = JsonSerializer.Deserialize<DepositCompletedEvent>(json);
                            if (@event is not null)
                            {
                                await dbContext.Notifications.AddAsync(new NotificationRecord
                                {
                                    UserId = @event.UserId,
                                    Title = "Deposit completed",
                                    Message = $"Deposit of {@event.Amount} {@event.Currency} completed successfully. Ref: {@event.TransactionReference}",
                                    NotificationType = NotificationType.Deposit
                                }, stoppingToken);
                            }

                            break;
                        }

                    case nameof(WithdrawCompletedEvent):
                        {
                            var @event = JsonSerializer.Deserialize<WithdrawCompletedEvent>(json);
                            if (@event is not null)
                            {
                                await dbContext.Notifications.AddAsync(new NotificationRecord
                                {
                                    UserId = @event.UserId,
                                    Title = "Withdraw completed",
                                    Message = $"Withdraw of {@event.Amount} {@event.Currency} completed successfully. Ref: {@event.TransactionReference}",
                                    NotificationType = NotificationType.Withdraw
                                }, stoppingToken);
                            }

                            break;
                        }

                    case nameof(TransferCompletedEvent):
                        {
                            var @event = JsonSerializer.Deserialize<TransferCompletedEvent>(json);
                            if (@event is not null)
                            {
                                await dbContext.Notifications.AddAsync(new NotificationRecord
                                {
                                    UserId = @event.UserId,
                                    Title = "Transfer completed",
                                    Message = $"Transfer of {@event.Amount} {@event.Currency} completed successfully. Ref: {@event.TransactionReference}",
                                    NotificationType = NotificationType.Transfer
                                }, stoppingToken);
                            }

                            break;
                        }
                }

                await dbContext.SaveChangesAsync(stoppingToken);

                _channel!.BasicAck(ea.DeliveryTag, false);
            }
            catch
            {
                _channel!.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel!.BasicConsume(
            queue: _settings.QueueName,
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }

    private static string GetEventType(IBasicProperties properties)
    {
        if (properties.Headers is null || !properties.Headers.TryGetValue("event-type", out var value) || value is null)
            return string.Empty;

        return value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            _ => value.ToString() ?? string.Empty
        };
    }
}
