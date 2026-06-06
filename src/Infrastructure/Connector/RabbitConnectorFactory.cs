using Application.Interfaces.Db;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Infrastructure.Connector;

public class RabbitConnectorFactory : IRabbitMqConnector
{
    private readonly ILogger<RabbitConnectorFactory> logger;
    public RabbitConnectorFactory(ILogger<RabbitConnectorFactory> logger)
    {
        this.logger = logger;        
    }
    public async Task<IConnection> CreateConnectionAsync(CancellationToken token)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq",
            Port = 5672,
            UserName = "admin",
            Password = "admin"
        };

        int count = 0;

        while (true)
        {
            try
            {
                count++;

                logger.LogError(
                    "create RabbitMQ connection: {count}", count);

                return await factory.CreateConnectionAsync(token);
            }
            catch
            {
                logger.LogError(
                    "RabbitMQ unavailable: {count}", count);

                await Task.Delay(5000, token);
            }
        }
    }
}