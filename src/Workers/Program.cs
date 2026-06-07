using Application.Interfaces.Services;
using Application.Interfaces.Db;
using Application.Features.Rows;
using Infrastructure.Connector;
using Infrastructure.Services;
using Infrastructure.Context;
using Workers.Consumers;
using Application;
using Infrastructure.Repositories;
using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Elastic.Clients.Elasticsearch;

namespace Workers;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        // Services:
        builder.Services.AddSingleton<RabbitConnectorFactory>();

        builder.Services.AddSingleton<IRabbitMqConnector>(provider =>
            provider.GetRequiredService<RabbitConnectorFactory>());

        builder.Services.AddDbContext<EndlessContext>(context => context.UseNpgsql(
            builder.Configuration.GetConnectionString("DB")));
        builder.Services.AddScoped<IAppDbContext>(provider =>
            provider.GetRequiredService<EndlessContext>());

        builder.Services.AddSingleton(sp =>
        {
            var settings = new ElasticsearchClientSettings(new Uri("http://search:9200"))
                .DefaultIndex("users");

            return new ElasticsearchClient(settings);
        });

        // Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // MediatR
        builder.Services.AddMediatR(cf =>
            cf.RegisterServicesFromAssembly(typeof(AppMaker).Assembly));

        // Depends
        builder.Services.AddSingleton<IFfmpegService, FfmpegService>();
        builder.Services.AddSingleton<IStorage, R2Service>();

        builder.Services.AddScoped<IChannelRepository, ChannelRepository>();
        builder.Services.AddScoped<IContentRepository, ContentRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

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

