using Application.Interfaces.Db;
using RabbitMQ.Client;

namespace Infrastructure.Connector;

public class RabbitConnectorFactory : IRabbitMqConnector
{
    public IConnection Connection { get; set; }

    public async Task CreateConnectionAsync()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq",
            Port = 5672,
            UserName = "admin",
            Password = "admin"
        };

        Connection = await factory.CreateConnectionAsync();
    }
}