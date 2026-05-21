using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Common.Interfaces.Services;
using Application.Contents.Dtos;
using Domain.Common.Interfaces.Db;
using Domain.Entities;
using MediatR;
using Application.Dtos;
using Application.Contents.Create;
using RabbitMQ.Client;
using Domain.Rows.Contents;

namespace Application.Contents.Update;

public class ContentUpdateHandler : IRequestHandler<ContentUpdateCommand, Result<ContentDto>>
{
    private readonly ILogger<ContentUpdateHandler> logger;
    private readonly IAppDbContext context;

    private readonly ContentUploadPublisher publisher;
    private readonly IR2Service r2Service;
    public ContentUpdateHandler(IAppDbContext context, ILogger<ContentUpdateHandler> logger, ContentUploadPublisher publisher, IR2Service r2Service)
    {
        this.context = context;
        this.logger = logger;

        this.publisher = publisher;
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

        string? videoPath = null;
        string? photoPath = null;

        if (request.ContentFile != null && request.ContentFile.Length != 0)
        {
            videoPath = await r2Service.SaveFormFileAsync(request.ContentFile, "Video", token: cancellationToken);
        }

        if (request.PrewievPhoto != null && request.PrewievPhoto.Length != 0)
        {
            photoPath = await r2Service.SaveFormFileAsync(request.PrewievPhoto, "Images", ".jpeg", cancellationToken);
        }

        content.c.Title = request.Title;
        content.c.ContentType = request.ContentType;

        // for local storage
        string path = content.c.VideoMeta.VideoUrl;

        if (!string.IsNullOrEmpty(path))
        {
            string? directoryName = Path.GetDirectoryName(path);

            if (directoryName != null)
                Directory.Delete(directoryName, true);
        }

        path = content.c.VideoMeta.PhotoUrl;

        if (!string.IsNullOrEmpty(path))
        {
            File.Delete(path);
        }

        await context.SaveChangesAsync();

        var message = new VideoUploadMessage(
            request.ContentId, content.c.Slug, videoPath, photoPath);

        await publisher.PublishAsync(message, cancellationToken);

        logger.LogInformation("Content {ContentId} updated successfully",
            request.ContentId);

        ContentDto contentDto = new(
            content.c.Id, content.c.ChannelId, content.c.CreatorId,
            content.c.Title, content.c.Slug, content.c.Description,
            content.c.CreatedDate, content.c.ContentType.ToString(),
            content.c.VideoMeta.DurationSeconds, content.c.VideoMeta.VideoUrl,
            new PreviewPhotoDto(content.c.VideoMeta.PhotoUrl, content.c.VideoMeta.R, content.c.VideoMeta.G, content.c.VideoMeta.B),
            content.SaversCount, content.LikersCount, content.CommentsCount, content.DisLikersCount, content.c.ViewsCount);

        return Result<ContentDto>.Success(200, contentDto);
    }
}