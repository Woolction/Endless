using Application.Interfaces.Services;
using Application.Interfaces.Db;
using Infrastructure.Connector;
using Workers.Consumers;
using Application;

namespace Workers;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        // Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // MediatR
        builder.Services.AddMediatR(cf =>
            cf.RegisterServicesFromAssembly(typeof(AppMaker).Assembly));

        // RabbitMQ
        builder.Services.AddSingleton<RabbitConnectorFactory>();

        builder.Services.AddSingleton<IRabbitMqConnector>(provider =>
            provider.GetRequiredService<RabbitConnectorFactory>());

        // Consumers
        builder.Services.AddTransient<IConsumer, SearchIndexUpsertingConsumer>();
        builder.Services.AddTransient<IConsumer, VideoUploadingConsumer>();
        builder.Services.AddTransient<IConsumer, IconUploadingConsumer>();

        var host = builder.Build();
        host.Run();
    }
}

