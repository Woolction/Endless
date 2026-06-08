using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Db;

namespace Messaging;

public static class DependencyInjection
{
    public static void AddMessagingInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<RabbitConnectorFactory>();

        services.AddSingleton<IRabbitMqConnector>(provider =>
            provider.GetRequiredService<RabbitConnectorFactory>());
    }
}
