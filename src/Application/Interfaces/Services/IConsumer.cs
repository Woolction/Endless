using RabbitMQ.Client;

namespace Application.Interfaces.Services;

public interface IConsumer : IDisposable
{
    Task Consume(IConnection connection, CancellationToken token);
}