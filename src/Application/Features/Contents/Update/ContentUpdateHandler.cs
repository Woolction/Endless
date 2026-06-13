using Application.Features.Contents.Video.Upload;
using Application.Features.Contents.Dtos;
using Application.Features.Rows.Contents;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Images;
using Application.Features.Images;
using Application.Interfaces.Db;
using Domain.Entities;
using MediatR;
using Domain.Common.Enums;

namespace Application.Features.Contents.Update;

public class ContentUpdateHandler : IRequestHandler<ContentUpdateCommand, Result<ContentDto>>
{
    private readonly ILogger<ContentUpdateHandler> logger;
    private readonly VideoUploadPublisher publisher;
    private readonly IAppDbContext context;
    private readonly IStorage Storage;

    public ContentUpdateHandler(IAppDbContext context, ILogger<ContentUpdateHandler> logger, VideoUploadPublisher publisher, IStorage Storage)
    {
        this.context = context;
        this.logger = logger;

        this.publisher = publisher;
        this.Storage = Storage;
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
            .Include(c => c.VideoMeta)
            .Select(content => new
            {
                c = content,
                SaversCount = content.Savers.Count,
                LikersCount = content.Likers.Count,
                CommentsCount = content.Comments.Count,
                DisLikersCount = content.DisLikers.Count
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (content == null)
            return Result<ContentDto>.Failure(404, "Content not found");

        content.c.Title = request.Title;
        content.c.ContentType = request.ContentType;

        string? videoPath = null;
        string? imagePath = null;

        if (request.ContentFile != null && request.ContentFile.Length != 0)
        {
            videoPath = await Storage.SaveFormFileAsync(
                request.ContentFile, "Video", token: cancellationToken);
        }

        if (request.PrewievPhoto != null && request.PrewievPhoto.Length != 0)
        {
            imagePath = await Storage.SaveFormFileAsync(
                request.PrewievPhoto, "Images", cancellationToken);
        }

        await context.SaveChangesAsync();

        // publishing to rabbit queue

        var message = new VideoUploadMessage(
            request.ContentId, content.c.Slug, videoPath, imagePath, ImageOwner.Content, ImageType.Preview);

        await publisher.PublishAsync(message, cancellationToken);

        logger.LogInformation("Content {ContentId} updated successfully",
            request.ContentId);

        ContentDto contentDto = new(
            content.c.Id, content.c.ChannelId, content.c.CreatorId,
            content.c.Title, content.c.Slug, content.c.Description,
            content.c.CreatedDate, content.c.ContentType.ToString(),
            content.c.VideoMeta.DurationSeconds, content.c.VideoMeta.VideoUrl,
            new ImageDto(
                new ImageVariantsDto(
                    content.c.VideoMeta.PhotoBase,
                    content.c.VideoMeta.Small,
                    content.c.VideoMeta.Medium,
                    content.c.VideoMeta.Large),
                content.c.VideoMeta.R,
                content.c.VideoMeta.G,
                content.c.VideoMeta.B),
            content.SaversCount, content.LikersCount, content.CommentsCount, content.DisLikersCount, content.c.ViewsCount);

        return Result<ContentDto>.Success(200, contentDto);
    }
}