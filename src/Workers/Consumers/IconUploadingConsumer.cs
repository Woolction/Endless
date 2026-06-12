using Application.Features.Images.Upload;
using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System.Text.Json;
using RabbitMQ.Client;
using System.Text;
using Application;
using MediatR;

namespace Workers.Consumers;

public class IconUploadingConsumer : IConsumer
{
    private readonly ILogger<IconUploadingConsumer> logger;
    private readonly IMediator mediator;
    private IChannel? channel;

    public IconUploadingConsumer(ILogger<IconUploadingConsumer> logger, IMediator mediator)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    public async Task Consume(IConnection connection, CancellationToken token)
    {
        channel = await connection.CreateChannelAsync(cancellationToken: token);

        await channel.QueueDeclareAsync(
            "icon.upload",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: token);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 4,
            global: false,
            token);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());

                var message = JsonSerializer.Deserialize<ImageUploadMessage>(body);

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
                    logger.LogError("failed to upload icon");

                    await channel.BasicNackAsync(
                        ea.DeliveryTag, false, false, token);

                    return;
                }

                logger.LogInformation("icon upload completed: send asc");

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
            queue: "icon.upload",
            autoAck: false,
            "icon.upload-consumer",
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