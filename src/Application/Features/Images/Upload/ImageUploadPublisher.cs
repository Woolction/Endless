using Application.Interfaces.Db;
using System.Text.Json;
using RabbitMQ.Client;
using System.Text;

namespace Application.Features.Images.Upload;

public class ImageUploadPublisher
{
    private readonly IRabbitMqConnector connector;
    public ImageUploadPublisher(IRabbitMqConnector connector)
    {
        this.connector = connector;
    }

    public async Task PublishAsync(ImageUploadMessage message, CancellationToken token)
    {
        var connection = await connector.CreateConnectionAsync(token);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: token);

        await channel.QueueDeclareAsync(
            queue: "icon.upload",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: token);

        string json = JsonSerializer.Serialize(message);

        byte[] body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties()
        {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "icon.upload",
            mandatory: true,
            properties,
            body: body,
            cancellationToken: token);
    }
}