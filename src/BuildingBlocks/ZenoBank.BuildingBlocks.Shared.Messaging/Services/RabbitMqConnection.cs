using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ZenoBank.BuildingBlocks.Shared.Messaging.Configurations;

namespace ZenoBank.BuildingBlocks.Shared.Messaging.Services;

public class RabbitMqConnection : IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqConnection> _logger;
    private IConnection? _connection;
    private readonly object _lock = new();

    public RabbitMqConnection(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqConnection> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        // ✅ Startup-da connection yaratmırıq - lazy edirik
        // Bəlkə RabbitMQ işləmir, bu Identity API-nin başlamasına mane olmamalıdır
    }

    public IConnection? TryGetConnection()
    {
        if (_connection?.IsOpen == true)
            return _connection;

        lock (_lock)
        {
            if (_connection?.IsOpen == true)
                return _connection;

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(3)
                };

                _connection = factory.CreateConnection();
                _logger.LogInformation("RabbitMQ connection established.");
                return _connection;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("RabbitMQ connection failed: {Message}. Events will not be published.", ex.Message);
                return null;
            }
        }
    }

    public void Dispose()
    {
        try
        {
            if (_connection?.IsOpen == true)
                _connection.Close();
            _connection?.Dispose();
        }
        catch { }
    }
}
