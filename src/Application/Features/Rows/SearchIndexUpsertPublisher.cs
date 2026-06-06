using Microsoft.Extensions.Logging;
using Application.Interfaces.Db;
using System.Text.Json;
using RabbitMQ.Client;
using System.Text;

namespace Application.Features.Rows;

public class SearchIndexUpsertPublisher
{
    private readonly ILogger<SearchIndexUpsertHandler> logger;
    private readonly IRabbitMqConnector connector;

    public SearchIndexUpsertPublisher(ILogger<SearchIndexUpsertHandler> logger, IRabbitMqConnector connector)
    {
        this.connector = connector;
        this.logger = logger;
    }

    public async Task Publish(SearchIndexUpsertMessage message, CancellationToken token)
    {
        var connection = await connector.CreateConnectionAsync(token);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: token);

        await channel.QueueDeclareAsync(
            "search_index.upsert", 
            true, 
            false, 
            false, 
            cancellationToken: token);

        var json = JsonSerializer.Serialize(message);

        var bytes = Encoding.UTF8.GetBytes(json);
        
        var properties = new BasicProperties()
        {
            Persistent = true
        };
        
        logger.LogInformation("publishing search index upsert message from channel {ChannelNumber}", channel.ChannelNumber);

        await channel.BasicPublishAsync(
            "", 
            "search_index.upsert", 
            true, 
            properties,
            bytes, 
            token);
    }
}