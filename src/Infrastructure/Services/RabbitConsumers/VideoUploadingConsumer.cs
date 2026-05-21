using Microsoft.Extensions.DependencyInjection;
using Domain.Common.Interfaces.Repositories;
using Domain.Common.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Common.Interfaces.Db;
using RabbitMQ.Client.Events;
using Domain.Rows.Contents;
using System.Text.Json;
using Domain.Entities;
using RabbitMQ.Client;
using System.Text;

namespace Infrastructure.Services.RabbitConsumers;

public class VideoUploadingConsumer : IConsumer
{
    private readonly ILogger<VideoUploadingConsumer> logger;
    private readonly IServiceScopeFactory scopeFactory;

    private readonly IFfmpegService ffmpegService;
    private readonly IR2Service r2Service;
    private IChannel? channel;

    public VideoUploadingConsumer(IServiceScopeFactory scopeFactory, ILogger<VideoUploadingConsumer> logger, IFfmpegService ffmpegService, IR2Service r2Service)
    {
        this.ffmpegService = ffmpegService;
        this.scopeFactory = scopeFactory;
        this.r2Service = r2Service;
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

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());

                VideoUploadMessage? message = JsonSerializer.Deserialize<VideoUploadMessage>(body);

                if (message == null)
                {
                    logger.LogError("message is null");

                    await channel.BasicNackAsync(
                        ea.DeliveryTag, false, false, token);

                    return;
                }

                string videoUrl = string.Empty;
                string photoUrl = string.Empty;

                double duration = default;

                if (message.PhotoPath != null)
                {
                    logger.LogInformation("save photo");

                    photoUrl = r2Service.SaveImage(message.PhotoPath, message.Slug);
                }

                if (message.VideoPath != null)
                {
                    logger.LogInformation("get video duration");

                    duration = await ffmpegService.GetVideoDuration(message.VideoPath, token);

                    double timeSeconds = Math.Clamp(20, 0, duration / 1.1f);

                    if (message.PhotoPath == null)
                    {
                        logger.LogInformation("get photo from video");

                        photoUrl = await ffmpegService.GetPhotoFromVideo(
                            message.VideoPath, message.Slug, timeSeconds: timeSeconds, token: token);
                    }

                    logger.LogInformation("uploading video");

                    videoUrl = await ffmpegService.UploadGeneratedVideos(message.VideoPath, message.Slug, token);
                }

                logger.LogInformation("save changes and create index");

                using var scope = scopeFactory.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                Content content = await context.Contents
                    .Include(c => c.VideoMeta)
                    .FirstAsync(c => c.Id == message.ContentId);

                // set photo 
                await content.VideoMeta.SetAverageColor(photoUrl, token);

                // set video
                content.VideoMeta.SetVideo(videoUrl, (int)duration);

                await context.SaveChangesAsync();

                await scope.ServiceProvider.GetRequiredService<IContentRepository>()
                   .CreateSearchIndex(content, content.VideoMeta, token);

                logger.LogInformation("Send Asc");

                await channel.BasicAckAsync(
                    ea.DeliveryTag, false, token);

                if (!string.IsNullOrEmpty(message.PhotoPath) && File.Exists(message.PhotoPath))
                    File.Delete(message.PhotoPath);

                if (!string.IsNullOrEmpty(message.VideoPath) && File.Exists(message.VideoPath))
                    File.Delete(message.VideoPath);

                Console.WriteLine("Video.Upload: process succed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"video uploding consumer: in task {ea.DeliveryTag} exception: {ex}");

                await channel.BasicNackAsync(
                    ea.DeliveryTag, false, false, token);
            }
        };

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            token);

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