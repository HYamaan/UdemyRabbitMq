using System.Data;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using RabbitMQ.Client.Events;
using Shared;
using IModel = RabbitMQ.Client.IModel;
using RabbitMQ.Client;

namespace FileCreateWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMqClientServices _rabbitMqClientServices;
        private readonly IServiceProvider _serviceProvider;
        private IModel _channel;

        public Worker(ILogger<Worker> logger, RabbitMqClientServices rabbitMqClientServices, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMqClientServices = rabbitMqClientServices;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
           _channel = _rabbitMqClientServices.Connect();
           _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            _channel.BasicConsume(RabbitMqClientServices.QueueName, false, consumer);
            consumer.Received += Consumer_Received;

            return Task.CompletedTask;

        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {

            try
            {
                var createExcelMessage =
                    JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

                using var ms = new MemoryStream();

                var wb = new XLWorkbook();
                var ds = new DataSet();

                ds.Tables.Add(GetTable("roducts"));
                wb.Worksheets.Add(ds);
                wb.SaveAs(ms);

                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(ms.ToArray()), "file", Guid.NewGuid().ToString() + ".xlsx");

                var baseAddress = "https://localhost:7240/api/files";
                using var client = new HttpClient();
                var response = await client.PostAsync($"{baseAddress}?fileId={createExcelMessage.FileId}", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"File created for {createExcelMessage.FileId}");
                    _channel.BasicAck(@event.DeliveryTag, false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private DataTable GetTable(string tableName)
        {
            List<FileCreateWorkerService.Models.Product> products;
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<AdventureWorks2022Context>();
                products = context.Products.ToList();
            }
            DataTable table = new DataTable(tableName);
            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("ProductNumber", typeof(string));
            table.Columns.Add("MakeFlag", typeof(bool));
            table.Columns.Add("FinishedGoodsFlag", typeof(bool));
            table.Columns.Add("Color", typeof(string));
            table.Columns.Add("SafetyStockLevel", typeof(short));
            table.Columns.Add("ReorderPoint", typeof(short));
            table.Columns.Add("StandardCost", typeof(decimal));
            table.Columns.Add("ListPrice", typeof(decimal));
            table.Columns.Add("Size", typeof(string));
            table.Columns.Add("SizeUnitMeasureCode", typeof(string));
            table.Columns.Add("WeightUnitMeasureCode", typeof(string));
            table.Columns.Add("Weight", typeof(decimal));
            table.Columns.Add("DaysToManufacture", typeof(int));
            table.Columns.Add("ProductLine", typeof(string));

            foreach (var product in products)
            {
                table.Rows.Add(product.ProductId, product.Name, product.ProductNumber, product.MakeFlag, product.FinishedGoodsFlag, product.Color, product.SafetyStockLevel, product.ReorderPoint, product.StandardCost, product.ListPrice, product.Size, product.SizeUnitMeasureCode, product.WeightUnitMeasureCode, product.Weight, product.DaysToManufacture, product.ProductLine);
            }
            return table;
        }
    }
}
