using RabbitMQ.Client;

namespace FileCreateWorkerService.Services;

public class RabbitMqClientServices : IDisposable
{
    private readonly ConnectionFactory _factory;
    private IConnection _connection;
    private IModel _channel;

    public static string QueueName = "queue-excel-files";

    private readonly ILogger<RabbitMqClientServices> _logger;

    public RabbitMqClientServices(ConnectionFactory factory, ILogger<RabbitMqClientServices> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public IModel Connect()
    {
        _connection = _factory.CreateConnection();
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }
        _channel = _connection.CreateModel();
        _logger.LogInformation("RabbitMq ile bağlantı kuruldu.");
        return _channel;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();

        _connection?.Close();
        _connection?.Dispose();
        _logger.LogInformation("RabbitMq ile bağlantı kapatıldı.");
    }
}