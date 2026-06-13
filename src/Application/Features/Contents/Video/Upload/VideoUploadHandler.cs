using Microsoft.Extensions.DependencyInjection;
using Application.Features.Rows.Contents;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Images;
using Application.Features.Rows;
using Application.Interfaces.Db;
using System.Text.Json;
using Domain.Entities;
using MediatR;

namespace Application.Features.Contents.Video.Upload;

public class VideoUploadHandler : IRequestHandler<VideoUploadMessage, Result<Null>>
{
    private readonly ILogger<VideoUploadHandler> logger;
    private readonly IServiceScopeFactory scopeFactory;

    private readonly SearchIndexUpsertPublisher publisher;
    private readonly IFfmpegService ffmpegService;
    private readonly IImageAnalyzer imageAnalyzer;
    private readonly IStorage Storage;

    public VideoUploadHandler(ILogger<VideoUploadHandler> logger, IServiceScopeFactory scopeFactory, SearchIndexUpsertPublisher publisher, IFfmpegService ffmpegService, IImageAnalyzer imageAnalyzer, IStorage Storage)
    {
        this.ffmpegService = ffmpegService;
        this.imageAnalyzer = imageAnalyzer;
        this.scopeFactory = scopeFactory;
        this.publisher = publisher;
        this.Storage = Storage;
        this.logger = logger;
    }

    public async Task<Result<Null>> Handle(VideoUploadMessage message, CancellationToken token)
    {
        ImageVariants photoUrl = new();

        string videoUrl = string.Empty;
        double duration = 0;

        if (message.PhotoPath != null)
        {
            logger.LogInformation("save photo");

            photoUrl = await Storage.SaveImageVariants(
                message.PhotoPath, message.Slug, token);
        }

        if (message.VideoPath != null)
        {
            logger.LogInformation("get video duration:");

            duration = await ffmpegService.GetVideoDuration(
                message.VideoPath, token);

            double timeSeconds = Math.Clamp(20, 0, duration / 1.1f); // 20 that from user message  

            logger.LogInformation("get video height:");

            int height = await ffmpegService.GetVideoHeight(
                message.VideoPath, token);

            if (message.PhotoPath == null)
            {
                logger.LogInformation("get photo from video:");

                photoUrl = await ffmpegService.GetPhotoFromVideo(
                    message.VideoPath, message.Slug, height, timeSeconds: timeSeconds, token: token);
            }

            logger.LogInformation("get video fps:");

            int fps = await ffmpegService.GetVideoFps(message.VideoPath, token);

            logger.LogInformation("generate video to m3u8");

            videoUrl = await ffmpegService.UploadGeneratedVideos(
                message.VideoPath, message.Slug, height, fps, token);
        }

        logger.LogInformation("save changes");

        await using var scope = scopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        Content content = await context.Contents
            .Include(c => c.VideoMeta)
            .FirstAsync(c => c.Id == message.ContentId);

        // set photo 
        // for local storage
        if (Directory.Exists(content.VideoMeta.PhotoBase))
        {
            Directory.Delete(content.VideoMeta.PhotoBase, true);
        }

        content.VideoMeta.SetPhoto(
            photoUrl.BaseUrl, photoUrl.Small, photoUrl.Medium, photoUrl.Large);
        await imageAnalyzer.SetAverageColor(
            Path.Combine(photoUrl.BaseUrl, photoUrl.Small), content.VideoMeta.SetColor, token);

        // set video
        // for local storage
        string? directoryName = Path.GetDirectoryName(content.VideoMeta.VideoUrl);

        if (directoryName != null)
            Directory.Delete(directoryName, true);

        content.VideoMeta.SetVideo(
            videoUrl, (int)duration);

        await context.SaveChangesAsync();

        // publish to upserting search index
        await publisher.Publish(
            new SearchIndexUpsertMessage(nameof(Content),
                JsonSerializer.Serialize(new ContentSearchIndex(content, content.VideoMeta))), token);

        // delete the files
        if (!string.IsNullOrEmpty(message.PhotoPath) && File.Exists(message.PhotoPath))
            File.Delete(message.PhotoPath);

        if (!string.IsNullOrEmpty(message.VideoPath) && File.Exists(message.VideoPath))
            File.Delete(message.VideoPath);

        logger.LogInformation("video upload complete: process succeed");

        return Result<Null>.Success(200, new Null());
    }
}