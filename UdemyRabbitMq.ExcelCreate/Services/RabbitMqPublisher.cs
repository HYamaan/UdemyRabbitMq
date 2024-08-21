using Newtonsoft.Json;
using System.Text;
using RabbitMQ.Client;
using Shared;

namespace UdemyRabbitMq.ExcelCreate.Services;

public class RabbitMqPublisher
{
    private readonly RabbitMqClientServices _rabbitMqClientServices;

    public RabbitMqPublisher(RabbitMqClientServices rabbitMqClientServices)
    {
        _rabbitMqClientServices = rabbitMqClientServices;
    }

    public void Publish(CreateExcelMessage createExcelMessage)
    {
        var channel = _rabbitMqClientServices.Connect();
        var bodyString = JsonConvert.SerializeObject(createExcelMessage);
        var bodyByte = Encoding.UTF8.GetBytes(bodyString);
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        channel.BasicPublish(RabbitMqClientServices.ExchangeName, RabbitMqClientServices.RoutingName, properties, bodyByte);

    }
}