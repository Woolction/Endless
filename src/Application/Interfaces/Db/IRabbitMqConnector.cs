using RabbitMQ.Client;

namespace Application.Interfaces.Db;

public interface IRabbitMqConnector
{
    public IConnection Connection { get; set; }

    Task CreateConnectionAsync();
}