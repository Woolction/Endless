using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Application.Features.Rows;
using RabbitMQ.Client.Events;
using System.Text.Json;
using RabbitMQ.Client;
using System.Text;
using Application;
using MediatR;

namespace Workers.Consumers;

public class SearchIndexUpsertingConsumer : IConsumer
{
    private readonly ILogger<SearchIndexUpsertingConsumer> logger;
    private readonly IMediator mediator;
    private IChannel? channel;

    public SearchIndexUpsertingConsumer(ILogger<SearchIndexUpsertingConsumer> logger, IMediator mediator)
    {
        this.mediator = mediator;
        this.logger = logger;
    }
    
    public async Task Consume(IConnection connection, CancellationToken token)
    {
        channel = await connection.CreateChannelAsync(cancellationToken: token);

        await channel.QueueDeclareAsync(
            "search_index.upsert", 
            true, 
            false, 
            false, 
            cancellationToken: token);

        await channel.BasicQosAsync(
            0, 
            8, 
            false, 
            token);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                string body = Encoding.UTF8.GetString(ea.Body.ToArray());
                
                var message = JsonSerializer.Deserialize<SearchIndexUpsertMessage>(body);

                if (message == null)
                {
                    logger.LogError("failed to deserialize message");
                    
                    await channel.BasicNackAsync(
                        ea.DeliveryTag, false, false, token);
                    
                    return;
                }

                Result<Null> result = await mediator.Send(message, token);

                if (!result.IsSuccess)
                {
                    logger.LogError("failed to upsert search index");
                    
                    await channel.BasicNackAsync(
                        ea.DeliveryTag, false, false, token);
                    
                    return;
                }
                
                await channel.BasicAckAsync(
                    ea.DeliveryTag, false, token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);

                await channel.BasicNackAsync(
                    ea.DeliveryTag, false, false, token);
            }
        };
        
        await channel.BasicConsumeAsync(
            queue: "search_index.upsert",
            autoAck: false,
            "search_index.upsert-consumer",
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer,
            token);
    }

    public void Dispose()
    {
        channel?.Dispose();
    }
}