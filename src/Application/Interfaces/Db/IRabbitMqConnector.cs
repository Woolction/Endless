using RabbitMQ.Client;

namespace Application.Interfaces.Db;

public interface IRabbitMqConnector
{
    Task<IConnection> CreateConnectionAsync(CancellationToken token);
}