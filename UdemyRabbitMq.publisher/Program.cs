using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://jxjiydev:GJ7PIC3hG0_JWAQ2_6oB4BQwYLM4qJVU@shrimp.rmq.cloudamqp.com/jxjiydev");
//factory.Uri = new Uri("amqp://localhost:5672");

using (var connection = factory.CreateConnection())
{
    var channel = connection.CreateModel();
    channel.QueueDeclare("demo-queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

    Enumerable.Range(0, 50).ToList().ForEach(i =>
        {
            string message = $"{i}";
            var messageBody = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(string.Empty, "demo-queue", null, messageBody);
            Console.WriteLine($"Mesaj:" + i );
        }
    );
}