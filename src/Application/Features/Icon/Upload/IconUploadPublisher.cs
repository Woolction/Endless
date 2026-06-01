using Application.Interfaces.Db;
using Application.Features.Icon.Upload;
using System.Text.Json;
using RabbitMQ.Client;
using System.Text;

namespace Application.Features.Icon.Upload;

public class IconUploadPublisher
{
    private readonly IRabbitMqConnector connector;
    public IconUploadPublisher(IRabbitMqConnector connector)
    {
        this.connector = connector;
    }
    
    public async Task PublishAsync(IconUploadMessage message, CancellationToken token)
    {
        await using var channel = await connector.Connection.CreateChannelAsync(cancellationToken: token);

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