using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text.Json;
using Shared;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://localhost:5672");

using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    channel.BasicQos(0, 1, false);
    EventingBasicConsumer consumer = new EventingBasicConsumer(channel);


    var queueName = channel.QueueDeclare().QueueName;

    Dictionary<string, object> headers = new Dictionary<string, object>();

    headers.Add("format", "pdf");
    headers.Add("shape", "a4");
    headers.Add("x-match", "all");  

    channel.QueueBind(queueName, "header-exchange", string.Empty,headers);

    channel.BasicConsume(queueName, false, consumer);
    Console.WriteLine("Mesajlar bekleniyor...");

    consumer.Received += (model, ea) =>
    {
        Thread.Sleep(1000);

        var message = Encoding.UTF8.GetString(ea.Body.ToArray());

        Product product = JsonSerializer.Deserialize<Product>(message);

        Console.WriteLine($"Gelen Mesaj: {product.Id}-{product.Name}-{product.Price}-{product.Stock}");

        channel.BasicAck(ea.DeliveryTag, false);

    };
    Console.ReadLine(); // Keep the application running
}
