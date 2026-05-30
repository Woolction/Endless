using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Features.Contents.Video.Upload;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Db;
using RabbitMQ.Client.Events;
using Application.Features.Rows.Contents;
using System.Text.Json;
using Domain.Entities;
using RabbitMQ.Client;
using System.Text;
using MediatR;

namespace Infrastructure.RabbitMQ.Consumers;

public class VideoUploadingConsumer : IConsumer
{
    private readonly ILogger<VideoUploadingConsumer> logger;
    private readonly IServiceScopeFactory scopeFactory;

    private readonly IFfmpegService ffmpegService;
    private readonly IR2Service r2Service;
    private readonly IMediator mediator;
    private IChannel? channel;

    public VideoUploadingConsumer(IServiceScopeFactory scopeFactory, IMediator mediator, ILogger<VideoUploadingConsumer> logger, IFfmpegService ffmpegService, IR2Service r2Service)
    {
        this.ffmpegService = ffmpegService;
        this.scopeFactory = scopeFactory;
        this.r2Service = r2Service;
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
                    logger.LogError("message is null");

                    await channel.BasicNackAsync(
                        ea.DeliveryTag, false, false, token);

                    return;
                }

                await mediator.Send(message, token);

                string videoUrl = string.Empty;
                PhotoVariants photoUrl = new();

                double duration = default;

                if (message.PhotoPath != null)
                {
                    logger.LogInformation("save photo");

                    photoUrl = await r2Service.SavePhotoVariants(
                        message.PhotoPath, message.Slug, token);
                }

                if (message.VideoPath != null)
                {
                    logger.LogInformation("get video duration");

                    duration = await ffmpegService.GetVideoDuration(
                        message.VideoPath, token);

                    double timeSeconds = Math.Clamp(20, 0, duration / 1.1f);

                    logger.LogInformation("get video height");

                    int height = await ffmpegService.GetVideoHeight(
                        message.VideoPath, token);

                    if (message.PhotoPath == null)
                    {
                        logger.LogInformation("get photo from video");

                        photoUrl = await ffmpegService.GetPhotoFromVideo(
                            message.VideoPath, message.Slug, height, timeSeconds: timeSeconds, token: token);
                    }

                    logger.LogInformation("get video fps");

                    int fps = await ffmpegService.GetVideoFps(message.VideoPath, token);

                    logger.LogInformation("uploading video");

                    videoUrl = await ffmpegService.UploadGeneratedVideos(
                        message.VideoPath, message.Slug, height, fps, token);
                }

                logger.LogInformation("save changes and create index");

                await using var scope = scopeFactory.CreateAsyncScope();

                var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                Content content = await context.Contents
                    .Include(c => c.VideoMeta)
                    .FirstAsync(c => c.Id == message.ContentId);

                // set photo 
                content.VideoMeta.SetPhoto(
                    photoUrl.BaseUrl, photoUrl.Small, photoUrl.Medium, photoUrl.Large);
                await content.VideoMeta.SetAverageColor(
                    photoUrl.Small, token);

                // set video
                content.VideoMeta.SetVideo(
                    videoUrl, (int)duration);

                await context.SaveChangesAsync();

                await scope.ServiceProvider.GetRequiredService<IContentRepository>()
                   .CreateSearchIndex(content, content.VideoMeta, token);

                logger.LogInformation("send asc");

                await channel.BasicAckAsync(
                    ea.DeliveryTag, false, token);

                if (!string.IsNullOrEmpty(message.PhotoPath) && File.Exists(message.PhotoPath))
                    File.Delete(message.PhotoPath);

                if (!string.IsNullOrEmpty(message.VideoPath) && File.Exists(message.VideoPath))
                    File.Delete(message.VideoPath);

                logger.LogInformation("process succed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"video uploding consumer: in task {ea.DeliveryTag} exception: {ex}");

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