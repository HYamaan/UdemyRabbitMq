using System.Drawing;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UdemyRabbitMqWeb.Watermark.Services;

namespace UdemyRabbitMqWeb.Watermark.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMqClientServices _rabbitMqClientServices;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
        private IModel _channel;


        public ImageWatermarkProcessBackgroundService(RabbitMqClientServices rabbitMqClientServices,
            ILogger<ImageWatermarkProcessBackgroundService> logger)
        {
            _rabbitMqClientServices = rabbitMqClientServices;
            _logger = logger;
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMqClientServices.Connect();
            _logger.LogInformation("ImageWatermarkProcessBackgroundService is started.");
            _channel.BasicQos(0, 1, false);



            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(RabbitMqClientServices.QueueName, false, consumer);
            _logger.LogInformation("Listening RabbitMq for Image Watermark Process.");

            consumer.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {

            try
            {
                var productImageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", productImageCreatedEvent.ImageFile);

                using var image = Image.FromFile(path);
                using var graphic = Graphics.FromImage(image);

                var font = new Font(FontFamily.GenericMonospace,50, FontStyle.Bold, GraphicsUnit.Pixel);
                var siteName = "www.mySite.com";
                var textSize = graphic.MeasureString(siteName, font);
                var color = Color.FromArgb(128, 255, 255, 255);
                var textBrush = new SolidBrush(color);
                var position = new Point(image.Width - ((int)textSize.Width + 30), image.Height - ((int)textSize.Height + 30));
                var text = new StringBuilder();
                text.Append(siteName);
                graphic.DrawString(text.ToString(), font, textBrush, position);

                image.Save("wwwroot/img/watermarks/" + productImageCreatedEvent.ImageFile);
                image.Dispose();
                graphic.Dispose();
                _channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
