using System.Text;
using RabbitMQ.Client;
using UdemyRabbitMq.publisher;

var factory = new ConnectionFactory();
//factory.Uri = new Uri("amqps://jxjiydev:GJ7PIC3hG0_JWAQ2_6oB4BQwYLM4qJVU@shrimp.rmq.cloudamqp.com/jxjiydev");
factory.Uri = new Uri("amqp://localhost:5672");

using (var connection = factory.CreateConnection())
{
    var channel = connection.CreateModel();
    //channel.QueueDeclare("demo-queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

    channel.ExchangeDeclare("log-direct",durable:true,type:ExchangeType.Direct);


    Enum.GetNames(typeof(LogNames)).ToList().ForEach(logName =>
    {
        var queueName = $"direct-queue-{logName}";
        var routeKey = $"route-{logName}";
        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBind(queueName, "log-direct", routeKey);
    });

    Enumerable.Range(0, 50).ToList().ForEach(i =>
        {
            LogNames logName = (LogNames)new Random().Next(1, 5);

            string message = $"log-type:{logName}";
            var messageBody = Encoding.UTF8.GetBytes(message);

            var routeKey = $"route-{logName}";

            channel.BasicPublish("log-direct", routeKey, null, messageBody);
            Console.WriteLine($"Log gönderilmiştir : {message}" );
        }
    );
}