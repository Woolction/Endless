using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Db;
using Domain.Rows.Icon.Upload;
using RabbitMQ.Client.Events;
using Domain.Rows.Contents;
using Domain.Common.Enums;
using System.Text.Json;
using RabbitMQ.Client;
using Domain.Entities;
using System.Text;
using MediatR;

namespace Infrastructure.RabbitMQ.Consumers;

public class IconUploadingConsumer : IConsumer
{
    private readonly ILogger<IconUploadingConsumer> logger;
    private readonly IServiceScopeFactory factory;
    private readonly IR2Service r2Service;
    private readonly IMediator mediator;
    private IChannel? channel;

    public IconUploadingConsumer(ILogger<IconUploadingConsumer> logger, IMediator mediator, IServiceScopeFactory factory, IR2Service r2Service)
    {
        this.r2Service = r2Service;
        this.mediator = mediator;
        this.factory = factory;
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

                var message = JsonSerializer.Deserialize<IconUploadMessage>(body);

                if (message == null)
                {
                    logger.LogError("message is null");

                    await channel.BasicNackAsync(
                        ea.DeliveryTag, false, false, token);

                    return;
                }

                //await mediator.Send(message, token);

                PhotoVariants iconVariants = await r2Service.SaveIconVariants(
                    message.PhotoPath, message.Slug, message.Type, token);

                await using var scope = factory.CreateAsyncScope();

                var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                if (message.Type == IconType.User)
                {
                    UserMeta? meta = await context.UserMetas
                        .Include(u => u.User)
                        .FirstOrDefaultAsync(u => u
                            .UserId == message.Id, token);

                    if (meta == null)
                    {
                        logger.LogError("user not found");

                        await channel.BasicNackAsync(
                            ea.DeliveryTag, false, false, token);

                        return;
                    }

                    meta.SetPhoto(iconVariants);
                    await meta.SetAverageColor(iconVariants.Small);

                    await context.SaveChangesAsync();

                    await scope.ServiceProvider.GetRequiredService<IUserRepository>()
                        .CreateSearchIndex(meta.User!, meta, token);
                }
                else if (message.Type == IconType.Channel)
                {
                    ChannelMeta? meta = await context.ChannelMetas
                        .Include(c => c.Channel)
                        .FirstOrDefaultAsync(c => c
                            .ChannelId == message.Id, token);

                    if (meta == null)
                    {
                        logger.LogError("channel not found");

                        await channel.BasicNackAsync(
                            ea.DeliveryTag, false, false, token);

                        return;
                    }

                    meta.SetPhoto(iconVariants);
                    await meta.SetAverageColor(iconVariants.Small);

                    await context.SaveChangesAsync();

                    await scope.ServiceProvider.GetRequiredService<IChannelRepository>()
                        .CreateSearchIndex(meta.Channel!, meta, token);
                }
    
                File.Delete(message.PhotoPath);

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