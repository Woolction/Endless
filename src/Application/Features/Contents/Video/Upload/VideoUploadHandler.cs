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
using Domain.Common.Enums;

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
        ImageVariantsDto imageVariants = new();

        string videoUrl = string.Empty;
        double duration = 0;

        if (message.ImagePath != null)
        {
            logger.LogInformation("save photo");

            imageVariants = await Storage.SaveImageVariants(
                message.ImagePath, message.Slug, [(1280, 720), (960, 540), (640, 360)],
                80, message.ImageOwner, message.ImageType, token);
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

            if (message.ImagePath == null)
            {
                logger.LogInformation("get photo from video:");

                imageVariants = await ffmpegService.GetPhotoFromVideo(
                    message.VideoPath, height, timeSeconds, message.Slug,
                    [(640, 360), (960, 540), (1280, 720)], 80, ImageOwner.Content,
                    ImageType.Preview, token: token);
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
            .Include(c => c.Meta)
                .ThenInclude(m => m.Image)
                    .ThenInclude(i => i.Variants)
            .FirstAsync(c => c.Id == message.ContentId, cancellationToken: token);

        // set photo 

        // for local storage

        if (Directory.Exists(content.Meta.Image.BaseUrl))
            Directory.Delete(content.Meta.Image.BaseUrl, true);

        await context.ImageVariants
            .Where(v => v.ImageId == content.Meta.Image.Id)
            .ExecuteDeleteAsync(token);

        await imageAnalyzer.SetImageVariants(
            content.Meta.Image, imageVariants, token);

        /* old
        if (Directory.Exists(content.Meta.PhotoBase))
            Directory.Delete(content.Meta.PhotoBase, true);

        content.Meta.SetPhoto(
            imageVariants.BaseUrl, imageVariants.Small, imageVariants.Medium, imageVariants.Large);
        await imageAnalyzer.SetAverageColor(
            Path.Combine(imageVariants.BaseUrl, imageVariants.Small), content.Meta.SetColor, token);*/

        // set video
        // for local storage
        string? directoryName = Path.GetDirectoryName(content.Meta.VideoUrl);

        if (directoryName != null)
            Directory.Delete(directoryName, true);

        content.Meta.SetVideo(
            videoUrl, (int)duration);

        await context.SaveChangesAsync();

        // publish to upserting search index
        await publisher.Publish(
            new SearchIndexUpsertMessage(nameof(Content),
                JsonSerializer.Serialize(new ContentSearchIndex(content, content.Meta, content.Meta.Image, content.Meta.Image.Variants))), token);

        // delete the files
        if (!string.IsNullOrEmpty(message.ImagePath) && File.Exists(message.ImagePath))
            File.Delete(message.ImagePath);

        if (!string.IsNullOrEmpty(message.VideoPath) && File.Exists(message.VideoPath))
            File.Delete(message.VideoPath);

        logger.LogInformation("video upload complete: process succeed");

        return Result<Null>.Success(200, new Null());
    }
}