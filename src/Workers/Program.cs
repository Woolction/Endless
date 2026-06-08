using Application.Interfaces.Services;
using Application.Features.Rows;
using Workers.Consumers;
using Persistence;
using Application;
using Messaging;
using Storage;
using Media;

namespace Workers;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        // Infrastructures:
        builder.Services.AddPersistenceInfrastructure(builder.Configuration);
        builder.Services.AddMessagingInfrastructure();
        builder.Services.AddStorageInfrastructure();
        builder.Services.AddMediaInfrastructure();

        // Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // MediatR
        builder.Services.AddMediatR(cf =>
            cf.RegisterServicesFromAssembly(typeof(AppMaker).Assembly));

        // Publishers
        builder.Services.AddSingleton<SearchIndexUpsertPublisher>();

        // Consumers
        builder.Services.AddTransient<IConsumer, SearchIndexUpsertingConsumer>();
        builder.Services.AddTransient<IConsumer, VideoUploadingConsumer>();
        builder.Services.AddTransient<IConsumer, IconUploadingConsumer>();

        var host = builder.Build();
        host.Run();
    }
}

