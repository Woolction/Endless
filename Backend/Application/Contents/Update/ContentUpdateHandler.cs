using Application.Contents.Dtos;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces.Services;

namespace Application.Contents.Update;

public class ContentUpdateHandler : IRequestHandler<ContentUpdateCommand, Result<ContentDto>>
{
    private readonly ILogger<ContentUpdateHandler> logger;
    private readonly IAppDbContext context;

    private readonly IFfmpegService ffmpegService;
    private readonly IR2Service r2Service;
    public ContentUpdateHandler(IAppDbContext context, ILogger<ContentUpdateHandler> logger, IFfmpegService ffmpegService, IR2Service r2Service)
    {
        this.context = context;
        this.logger = logger;

        this.ffmpegService = ffmpegService;
        this.r2Service = r2Service;
    }
    
    public async Task<Result<ContentDto>> Handle(ContentUpdateCommand request, CancellationToken cancellationToken)
    {
        User? user = await context.Users.FindAsync(request.UserId, cancellationToken);

        if (user == null)
            return Result<ContentDto>.Failure(404, "User not found");

        var content = await context.Contents
            .Where(content =>
                content.Id == request.ContentId &&
                content.CreatorId == request.UserId)
            .Select(content => new
            {
                c = content,
                SaversCount = content.Savers.Count,
                LikersCount = content.Likers.Count,
                CommentsCount = content.Comments.Count,
                DisLikersCount = content.DisLikers.Count,
                ViewsCount = content.ViewsCount
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (content == null)
            return Result<ContentDto>.Failure(404, "Content not found");

        VideoMetaData? videoMetaData = await context.VideoMetas
            .FirstOrDefaultAsync(videoMeta => videoMeta.ContentId == request.ContentId, cancellationToken);

        string videoUrl = null!;
        string videoPath = null!;

        if (request.ContentFile != null && request.ContentFile.Length != 0)
        {
            videoPath = await r2Service.SaveFormFileAsync(request.ContentFile, "Video");
            videoUrl = await ffmpegService.UploadGeneratedVideos(videoPath);
        }

        string photoUrl = null!;

        if (request.PrewievPhoto != null && request.PrewievPhoto.Length != 0)
        {
            string photoPath = await r2Service.SaveFormFileAsync(request.PrewievPhoto, "Images", ".jpeg");
            photoUrl = await r2Service.SaveImage(photoPath);

            File.Delete(photoPath);
        }

        content.c.Title = request.Title;
        content.c.ContentUrl = videoUrl;
        content.c.PrewievPhotoUrl = photoUrl;
        content.c.ContentType = request.ContentType;

        if (videoMetaData != null && videoPath != null)
        {
            videoMetaData.ContentId = request.ContentId;
            videoMetaData.DurationSeconds = await ffmpegService.GetVideoDuration(videoPath);

            File.Delete(videoPath);
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Content {ContentId} updated successfully",
            request.ContentId);

        ContentDto contentDto = new(
            content.c.Id, content.c.ChannelId, content.c.CreatorId,
            content.c.Title, content.c.Slug, content.c.Description,
            content.c.CreatedDate, content.c.ContentType.ToString(),
            content.c.VideoMeta == null ? 0 : content.c.VideoMeta.DurationSeconds,
            content.c.ContentUrl, content.c.PrewievPhotoUrl, content.SaversCount,
            content.LikersCount, content.CommentsCount, content.DisLikersCount,
            content.ViewsCount);

        return Result<ContentDto>.Success(200, contentDto);
    }
}