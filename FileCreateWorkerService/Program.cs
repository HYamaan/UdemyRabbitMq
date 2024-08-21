using FileCreateWorkerService;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration Configuration = hostContext.Configuration;

        services.AddDbContext<AdventureWorks2022Context>(options =>
        {
            options.UseSqlServer(Configuration.GetConnectionString("SqlConnection"));
        });

        services.AddHostedService<Worker>();
        services.AddSingleton<RabbitMqClientServices>();
        services.AddSingleton(sp => new ConnectionFactory()
        {
            Uri = new Uri(Configuration.GetConnectionString("RabbitMqConnection")),
            DispatchConsumersAsync = true
        });
    })
    .Build();

host.Run();
