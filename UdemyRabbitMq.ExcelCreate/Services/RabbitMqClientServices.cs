using RabbitMQ.Client;

namespace UdemyRabbitMq.ExcelCreate.Services
{
    public class RabbitMqClientServices : IDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;

        public static string ExchangeName = "ExcelDirectExchange";
        public static string RoutingName = "excel-route-files";
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
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, true, false, null);
            _channel.QueueDeclare(QueueName, true, false, false, null);
            _channel.QueueBind(QueueName, ExchangeName, RoutingName);
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
}
