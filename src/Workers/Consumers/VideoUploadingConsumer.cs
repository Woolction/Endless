using Application.Features.Contents.Video.Upload;
using Application.Interfaces.Services;
using RabbitMQ.Client.Events;
using System.Text.Json;
using RabbitMQ.Client;
using System.Text;
using Application;
using MediatR;

namespace Workers.Consumers;

public class VideoUploadingConsumer : IConsumer
{
    private readonly ILogger<VideoUploadingConsumer> logger;
    private readonly IMediator mediator;
    private IChannel? channel;

    public VideoUploadingConsumer(IMediator mediator, ILogger<VideoUploadingConsumer> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    public async Task Consume(IConnection connection, CancellationToken token)
    {
        channel = await connection.CreateChannelAsync(cancellationToken: token);

        await channel.QueueDeclareAsync(
            queue: "video.upload",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: token);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            token);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());

                VideoUploadMessage? message = JsonSerializer.Deserialize<VideoUploadMessage>(body);

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
                    logger.LogError("failed to upload video");
                    
                    await channel.BasicNackAsync(
                        ea.DeliveryTag, false, false, token);
                }

                logger.LogInformation("video upload completed: send asc");

                await channel.BasicAckAsync(
                    ea.DeliveryTag, false, token);
            }
            catch (Exception ex)
            {
                logger.LogError("video uploding consumer: in task {DeliveryTag} exception: {ex}", ea.DeliveryTag, ex);

                await channel.BasicNackAsync(
                    ea.DeliveryTag, false, false, token);
            }
        };

        await channel.BasicConsumeAsync(
            queue: "video.upload",
            autoAck: false,
            "video.upload-consumer",
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