using Application.Contents.Dtos;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Contents.Create.ForUser;

public class ContentCreateForUserHandler : IRequestHandler<ContentCreateForUserCommand, Result<ContentDto>>
{
    private readonly IAppDbContext context;

    private readonly IContentRepository contentRepository;
    private readonly ILogger<ContentCreateForUserHandler> logger;
    private readonly IFfmpegService ffmpegService;
    private readonly IR2Service r2Service;

    public ContentCreateForUserHandler(IAppDbContext context, IContentRepository contentRepository, IFfmpegService ffmpegService, IR2Service r2Service, ILogger<ContentCreateForUserHandler> logger)
    {
        this.context = context;

        this.contentRepository = contentRepository;
        this.ffmpegService = ffmpegService;
        this.r2Service = r2Service;
        this.logger = logger;
    }

    public async Task<Result<ContentDto>> Handle(ContentCreateForUserCommand cmd, CancellationToken cancellationToken)
    {
        User? user = await context.Users.FindAsync(
            cmd.UserId, cancellationToken);

        if (user == null)
            return Result<ContentDto>.Failure(404, "User not found");

        string videoUrl = null!;
        string videoPath = null!;

        if (cmd.ContentFile != null && cmd.ContentFile.Length != 0)
        {
            videoPath = await r2Service.SaveFormFileAsync(cmd.ContentFile, "Video");
            videoUrl = await ffmpegService.UploadGeneratedVideos(videoPath);
        }

        string photoUrl = null!;

        if (cmd.PrewievPhoto != null && cmd.PrewievPhoto.Length != 0)
        {
            string photoPath = await r2Service.SaveFormFileAsync(cmd.PrewievPhoto, "Images", ".jpeg");
            photoUrl = await r2Service.SaveImage(photoPath);

            File.Delete(photoPath);
        }

        Content content = new()
        {
            CreatorId = cmd.UserId,
            Title = cmd.Title,
            Slug = Guid.NewGuid(),
            ContentUrl = videoUrl,
            PrewievPhotoUrl = photoUrl,
            CreatedDate = DateTime.UtcNow,
            RandomKey = System.Random.Shared.NextDouble(),
            ContentType = cmd.ContentType
        };

        int duration = await GetVideoDuration(videoPath);

        VideoMetaData metaData = new()
        {
            Content = content,
            DurationSeconds = duration
        };

        context.VideoMetas.Add(metaData);
        context.Contents.Add(content);
        context.ContentVectors.AddRange(await context.Genres
            .Select(genre => new ContentGenreVector()
            {
                Content = content,
                GenreId = genre.Id
            })
            .AsNoTracking()
            .ToArrayAsync(cancellationToken)
        );

        await context.SaveChangesAsync();

        await contentRepository.CreateSearchIndex(content, metaData, cancellationToken);

        logger.LogInformation("content {ContentId} created has user {UserId}",
            content.Id, cmd.UserId);

        return Result<ContentDto>.Success(201, new ContentDto(
            content.Id, content.ChannelId, content.CreatorId,
            content.Title, content.Slug, content.Description,
            content.CreatedDate, content.ContentType.ToString(), duration,
            content.ContentUrl, content.PrewievPhotoUrl, 0, 0, 0, 0, 0));
    }

    private async Task<int> GetVideoDuration(string? videoPath)
    {
        if (videoPath != null)
        {
            int duration = await ffmpegService.GetVideoDuration(videoPath);

            File.Delete(videoPath);

            return duration;
        }

        return 0;
    }
}