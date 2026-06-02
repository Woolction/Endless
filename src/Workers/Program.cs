using Application;
using Application.Interfaces.Services;
using Workers.Consumers;

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

        // Consumers
        builder.Services.AddTransient<IConsumer, SearchIndexUpsertingConsumer>();
        builder.Services.AddTransient<IConsumer, VideoUploadingConsumer>();
        builder.Services.AddTransient<IConsumer, IconUploadingConsumer>();

        var host = builder.Build();
        host.Run();
    }
}

