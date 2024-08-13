using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://jxjiydev:GJ7PIC3hG0_JWAQ2_6oB4BQwYLM4qJVU@shrimp.rmq.cloudamqp.com/jxjiydev");


    using (var connection = factory.CreateConnection())
    using (var channel = connection.CreateModel())
    {
        channel.BasicQos(0, 1, false);

        var consumer = new EventingBasicConsumer(channel);
        channel.BasicConsume("demo-queue", false, consumer);

        consumer.Received += (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine($"Received: {message}");
                Thread.Sleep(1500);

                channel.BasicAck(ea.DeliveryTag, false);
                
        };

        Console.WriteLine("Press [enter] to exit.");
        Console.ReadLine(); // Keep the application running
    }
