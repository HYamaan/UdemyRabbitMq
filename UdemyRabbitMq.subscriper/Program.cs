﻿using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

var factory = new ConnectionFactory();
//factory.Uri = new Uri("amqps://jxjiydev:GJ7PIC3hG0_JWAQ2_6oB4BQwYLM4qJVU@shrimp.rmq.cloudamqp.com/jxjiydev");
factory.Uri = new Uri("amqp://localhost:5672");

using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    channel.BasicQos(0, 1, false);
    EventingBasicConsumer consumer = new EventingBasicConsumer(channel);


    var queueName = "direct-queue-Critical";
    channel.BasicConsume(queueName, false, consumer);


    consumer.Received += (model, ea) =>
    {
        Thread.Sleep(1000);

        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
        Console.WriteLine($"Gelen Mesaj: {message}");
        //File.AppendAllText("logs.txt", message + Environment.NewLine);

        channel.BasicAck(ea.DeliveryTag, false);

    };

    Console.WriteLine("Press [enter] to exit.");
    Console.ReadLine(); // Keep the application running
}
