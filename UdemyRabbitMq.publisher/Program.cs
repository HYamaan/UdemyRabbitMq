﻿using System.Text;
using RabbitMQ.Client;
using UdemyRabbitMq.publisher;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://localhost:5672");

using (var connection = factory.CreateConnection())
{
    var channel = connection.CreateModel();
    channel.ExchangeDeclare("log-topic", durable: true, type: ExchangeType.Topic);

    Random rnd = new Random();
    Enumerable.Range(0, 50).ToList().ForEach(i =>
        {
            LogNames log1 = (LogNames)rnd.Next(1, 5);
            LogNames log2 = (LogNames)rnd.Next(1, 5);
            LogNames log3 = (LogNames)rnd.Next(1, 5);
            var routeKey = $"{log1}.{log2}.{log3}";

            string message = $"{log1}-{log2}-{log3}";
            var messageBody = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish("log-topic", routeKey, null, messageBody);
            Console.WriteLine($"Log gönderilmiştir : {message}");
        }
    );
}