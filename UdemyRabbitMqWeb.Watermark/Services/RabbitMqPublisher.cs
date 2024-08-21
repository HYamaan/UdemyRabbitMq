using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace UdemyRabbitMqWeb.Watermark.Services
{
    public class RabbitMqPublisher
    {
        private readonly RabbitMqClientServices _rabbitMqClientServices;

        public RabbitMqPublisher(RabbitMqClientServices rabbitMqClientServices)
        {
            _rabbitMqClientServices = rabbitMqClientServices;
        }

        public void PublishProductImageCreatedEvent(ProductImageCreatedEvent productImageCreatedEvent)
        {
           var channel = _rabbitMqClientServices.Connect();
           var bodyString = JsonConvert.SerializeObject(productImageCreatedEvent);
           var bodyByte = Encoding.UTF8.GetBytes(bodyString);
           var properties = channel.CreateBasicProperties();
           properties.Persistent = true;
           channel.BasicPublish(RabbitMqClientServices.ExchangeName, RabbitMqClientServices.RoutingName, properties, bodyByte);

        }
    }
}
