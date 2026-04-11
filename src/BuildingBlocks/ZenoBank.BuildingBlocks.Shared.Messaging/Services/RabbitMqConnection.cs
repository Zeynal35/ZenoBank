using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ZenoBank.BuildingBlocks.Shared.Messaging.Configurations;

namespace ZenoBank.BuildingBlocks.Shared.Messaging.Services;

public class RabbitMqConnection : IDisposable
{
    private readonly IConnection _connection;

    public RabbitMqConnection(IOptions<RabbitMqSettings> settings)
    {
        var s = settings.Value;
        var factory = new ConnectionFactory
        {
            HostName = s.HostName,
            Port = s.Port,
            UserName = s.UserName,
            Password = s.Password
        };

        _connection = factory.CreateConnection();
    }

    public IConnection GetConnection() => _connection;

    public void Dispose()
    {
        if (_connection.IsOpen)
            _connection.Close();

        _connection.Dispose();
    }
}   
