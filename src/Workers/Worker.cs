using Application.Interfaces.Services;
using Application.Interfaces.Db;

namespace Workers;

public class Worker : BackgroundService
{
    private readonly IRabbitMqConnector connector;
    private readonly IConsumer[] consumers;

    public Worker(IRabbitMqConnector connector, IEnumerable<IConsumer> consumers)
    {
        this.connector = connector;
        this.consumers = [.. consumers];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await connector.CreateConnectionAsync(stoppingToken);

        for (int i = 0; i < consumers.Length; i++)
        {
            await consumers[i].Consume(connection, stoppingToken);
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);

        for (int i = 0; i < consumers.Length; i++)
        {
            consumers[i].Dispose();
        }
    }
}
