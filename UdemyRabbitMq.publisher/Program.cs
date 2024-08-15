using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared;
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
    properties.Persistent = true; // Message will be saved to disk

    var product = new Product {Id=1, Name = "Laptop", Price = 1000,Stock = 10};
    var productJson = JsonSerializer.Serialize(product);

    var message = "header message";
    var body = Encoding.UTF8.GetBytes(productJson);
    channel.BasicPublish("header-exchange",string.Empty,properties,body);


    Console.WriteLine("Mesaj gönderildi");
    Console.ReadLine();
}