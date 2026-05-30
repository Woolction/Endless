using Application.Features.Rows.Contents.Video.Upload;
using Application.Interfaces.Db;
using System.Text.Json;
using RabbitMQ.Client;
using System.Text;

namespace Application.Features.Contents.Video.Upload;

public class VideoUploadPublisher
{
    private readonly IRabbitMqConnector connection;
    public VideoUploadPublisher(IRabbitMqConnector connection)
    {
        this.connection = connection;
    }

    public async Task PublishAsync(VideoUploadMessage message, CancellationToken cancellationToken)
    {
        using var channel = await connection.Connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: "video.upload",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(message);

        byte[] body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties()
        {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "video.upload",
            mandatory: true,
            properties,
            body: body,
            cancellationToken);
    }
}