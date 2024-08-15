using System.Text;
using RabbitMQ.Client;
using UdemyRabbitMq.publisher;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://localhost:5672");

using (var connection = factory.CreateConnection())
{
    var channel = connection.CreateModel();
    channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);

    Dictionary<string,object> headers = new Dictionary<string, object>();
    headers.Add("format", "pdf");
    headers.Add("shape", "a4");

    var properties = channel.CreateBasicProperties();
    properties.Headers = headers;

    var message = "header message";
    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish("header-exchange",string.Empty,properties,body);


    Console.WriteLine("Mesaj gönderildi");
    Console.ReadLine();
}