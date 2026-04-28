using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ZenoBank.BuildingBlocks.Shared.Contracts.Events;
using ZenoBank.BuildingBlocks.Shared.Messaging.Configurations;
using ZenoBank.BuildingBlocks.Shared.Messaging.Services;
using ZenoBank.Services.Notification.Application.Abstractions.Services;
using ZenoBank.Services.Notification.Domain.Entities;
using ZenoBank.Services.Notification.Domain.Enums;
using ZenoBank.Services.Notification.Infrastructure.Persistence;

namespace ZenoBank.Services.Notification.Infrastructure.BackgroundWorkers;

public class TransactionEventsConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqConnection _connection;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<TransactionEventsConsumer> _logger;
    private IModel? _channel;

    public TransactionEventsConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMqConnection connection,
        IOptions<RabbitMqSettings> settings,
        ILogger<TransactionEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _connection = connection;
        _settings = settings.Value;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = _connection.TryGetConnection();
        if (connection is null)
        {
            _logger.LogWarning("RabbitMQ unavailable. TransactionEventsConsumer will not start.");
            return Task.CompletedTask;
        }

        _channel = connection.CreateModel();

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

                _logger.LogInformation("Received event: {EventType}", eventType);

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                var identityClient = scope.ServiceProvider.GetRequiredService<IIdentityServiceClient>();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                Guid? userId = null;
                string title = string.Empty;
                string message = string.Empty;
                string emailSubject = string.Empty;
                string emailBody = string.Empty;
                string directEmail = string.Empty;
                string directUserName = string.Empty;
                NotificationType notificationType = NotificationType.Deposit;
                var createInAppNotification = true;

                switch (eventType)
                {
                    case nameof(DepositCompletedEvent):
                        {
                            var @event = JsonSerializer.Deserialize<DepositCompletedEvent>(json);
                            if (@event is null) break;
                            userId = @event.UserId;
                            title = "Deposit completed";
                            message = $"Deposit of {@event.Amount} {@event.Currency} completed successfully. Ref: {@event.TransactionReference}";
                            emailSubject = "ZenoBank - Deposit notification";
                            emailBody = BuildEmailHtml("Deposit completed", message);
                            notificationType = NotificationType.Deposit;
                            break;
                        }

                    case nameof(WithdrawCompletedEvent):
                        {
                            var @event = JsonSerializer.Deserialize<WithdrawCompletedEvent>(json);
                            if (@event is null) break;
                            userId = @event.UserId;
                            title = "Withdraw completed";
                            message = $"Withdraw of {@event.Amount} {@event.Currency} completed successfully. Ref: {@event.TransactionReference}";
                            emailSubject = "ZenoBank - Withdraw notification";
                            emailBody = BuildEmailHtml("Withdraw completed", message);
                            notificationType = NotificationType.Withdraw;
                            break;
                        }

                    case nameof(TransferCompletedEvent):
                        {
                            var @event = JsonSerializer.Deserialize<TransferCompletedEvent>(json);
                            if (@event is null) break;
                            userId = @event.UserId;
                            title = "Transfer completed";
                            message = $"Transfer of {@event.Amount} {@event.Currency} completed successfully. Ref: {@event.TransactionReference}";
                            emailSubject = "ZenoBank - Transfer notification";
                            emailBody = BuildEmailHtml("Transfer completed", message);
                            notificationType = NotificationType.Transfer;
                            break;
                        }

                    case nameof(UserLoggedInEvent):
                        {
                            var @event = JsonSerializer.Deserialize<UserLoggedInEvent>(json);
                            if (@event is null) break;
                            userId = @event.UserId;
                            title = "Login detected";
                            message = $"A login occurred on your ZenoBank account at {@event.LoggedInAtUtc:u}. If this was not you, contact support immediately.";
                            emailSubject = "ZenoBank security alert - Login detected";
                            emailBody = BuildEmailHtml("Login detected", message);
                            notificationType = NotificationType.Security;
                            break;
                        }

                    case nameof(UserLoggedOutEvent):
                        {
                            var @event = JsonSerializer.Deserialize<UserLoggedOutEvent>(json);
                            if (@event is null) break;
                            userId = @event.UserId;
                            title = "Logout completed";
                            message = $"Your ZenoBank account was logged out at {@event.LoggedOutAtUtc:u}.";
                            emailSubject = "ZenoBank - Logout notification";
                            emailBody = BuildEmailHtml("Logout completed", message);
                            notificationType = NotificationType.Security;
                            break;
                        }

                    case nameof(AccountFrozenEvent):
                        {
                            var @event = JsonSerializer.Deserialize<AccountFrozenEvent>(json);
                            if (@event is null) break;
                            userId = @event.UserId;
                            title = "Account frozen";
                            message = $"Your bank account {@event.AccountNumber} was frozen at {@event.FrozenAtUtc:u}.";
                            emailSubject = "ZenoBank - Account frozen";
                            emailBody = BuildEmailHtml("Account frozen", message);
                            notificationType = NotificationType.Account;
                            break;
                        }

                    case nameof(AccountUnfrozenEvent):
                        {
                            var @event = JsonSerializer.Deserialize<AccountUnfrozenEvent>(json);
                            if (@event is null) break;
                            userId = @event.UserId;
                            title = "Account unfrozen";
                            message = $"Your bank account {@event.AccountNumber} was unfrozen at {@event.UnfrozenAtUtc:u}.";
                            emailSubject = "ZenoBank - Account unfrozen";
                            emailBody = BuildEmailHtml("Account unfrozen", message);
                            notificationType = NotificationType.Account;
                            break;
                        }

                    case nameof(LoanApprovedEvent):
                        {
                            var @event = JsonSerializer.Deserialize<LoanApprovedEvent>(json);
                            if (@event is null) break;
                            userId = @event.UserId;
                            title = "Loan approved";
                            message = $"Your loan request was approved. Principal: {@event.PrincipalAmount} {@event.Currency}, Interest: {@event.InterestRate}%, Monthly payment: {@event.MonthlyPayment} {@event.Currency}.";
                            emailSubject = "ZenoBank - Loan approved";
                            emailBody = BuildEmailHtml("Loan approved", message);
                            notificationType = NotificationType.Loan;
                            break;
                        }

                    case nameof(LoanRejectedEvent):
                        {
                            var @event = JsonSerializer.Deserialize<LoanRejectedEvent>(json);
                            if (@event is null) break;
                            userId = @event.UserId;
                            title = "Loan rejected";
                            message = $"Your loan request was rejected. Reason: {@event.Reason}";
                            emailSubject = "ZenoBank - Loan rejected";
                            emailBody = BuildEmailHtml("Loan rejected", message);
                            notificationType = NotificationType.Loan;
                            break;
                        }

                    case nameof(EmailVerificationRequestedEvent):
                        {
                            var @event = JsonSerializer.Deserialize<EmailVerificationRequestedEvent>(json);
                            if (@event is null) break;
                            directEmail = @event.Email;
                            directUserName = @event.UserName;
                            emailSubject = "ZenoBank - Your verification code";
                            emailBody = BuildEmailHtml(
                                "Verify your email",
                                $"Hello {@event.UserName},<br/><br/>" +
                                $"Your ZenoBank email verification code is:<br/><br/>" +
                                $"<div style=\"font-size:36px;font-weight:bold;letter-spacing:12px;color:#1a56db;padding:16px 0;\">{@event.VerificationToken}</div><br/>" +
                                $"Enter this 6-digit code on the verification page.<br/><br/>" +
                                $"<b>This code expires in 15 minutes.</b><br/><br/>" +
                                $"If you did not register for ZenoBank, please ignore this email.");
                            createInAppNotification = false;
                            break;
                        }
                }

                if (createInAppNotification && userId is not null && !string.IsNullOrWhiteSpace(title))
                {
                    await dbContext.Notifications.AddAsync(new NotificationRecord
                    {
                        UserId = userId.Value,
                        Title = title,
                        Message = message,
                        NotificationType = notificationType
                    }, stoppingToken);

                    await dbContext.SaveChangesAsync(stoppingToken);
                }

                if (!string.IsNullOrWhiteSpace(directEmail))
                {
                    _logger.LogInformation("Sending email directly to {Email}", directEmail);
                    await emailSender.SendAsync(directEmail, directUserName, emailSubject, emailBody, stoppingToken);
                    _logger.LogInformation("Email sent successfully to {Email}", directEmail);
                }
                else if (userId is not null)
                {
                    var userResult = await identityClient.GetUserContactAsync(userId.Value, stoppingToken);
                    if (userResult.IsSuccess && userResult.Data is not null &&
                        !string.IsNullOrWhiteSpace(userResult.Data.Email))
                    {
                        _logger.LogInformation("Sending email to {Email} (EmailConfirmed={Confirmed})",
                            userResult.Data.Email, userResult.Data.EmailConfirmed);

                        await emailSender.SendAsync(
                            userResult.Data.Email,
                            userResult.Data.UserName,
                            emailSubject,
                            emailBody,
                            stoppingToken);

                        _logger.LogInformation("Email sent successfully to {Email}", userResult.Data.Email);
                    }
                }

                _channel!.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RabbitMQ event");
                _channel!.BasicNack(ea.DeliveryTag, false, false);
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
        _channel?.Dispose();
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

    private static string BuildEmailHtml(string title, string message)
    {
        return $"""
                <html>
                <body style="font-family:Arial,sans-serif;">
                    <h2>{title}</h2>
                    <p>{message}</p>
                    <p>Regards,<br/>ZenoBank</p>
                </body>
                </html>
                """;
    }
}