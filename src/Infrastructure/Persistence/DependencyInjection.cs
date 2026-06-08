using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Db;
using Persistence.Repositories;
using Persistence.Context;

namespace Persistence;

public static class DependencyInjection
{
    public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string DbKey = configuration.GetConnectionString("DB")!;

        services.AddDbContext<EndlessContext>(context =>
            context.UseNpgsql(DbKey));

        services.AddScoped<IAppDbContext>(provider =>
            provider.GetRequiredService<EndlessContext>());

        services.AddSingleton<IDbConnector, DbConnectorFactory>();

        services.AddSingleton(sp =>
        {
            var settings = new ElasticsearchClientSettings(new Uri("http://search:9200"))
                .DefaultIndex("users");

            return new ElasticsearchClient(settings);
        });

        services.AddRepositories();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserVectorsRepository, UserVectorsRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }
}