using Application.Channels.Dtos;
using Application.Contents.Dtos;
using Application.Users.Dtos;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Contents.Choose;

public class ContentChooseHandler : IRequestHandler<ContentChooseQuery, Result<ContentDto>>
{
    private readonly ILogger<ContentChooseHandler> logger;
    private readonly IAppDbContext context;

    public ContentChooseHandler(IAppDbContext context, ILogger<ContentChooseHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<Result<ContentDto>> Handle(ContentChooseQuery query, CancellationToken cancellationToken)
    {
        ContentDto? changedContent = await context.Contents
            .AsNoTracking()
            .Where(content => content.Id == query.ContentId)
            .Select(content => new ContentDto(content.Id, content.ChannelId, content.CreatorId,
                    content.Title, content.Slug, content.Description,
                    content.CreatedDate, content.ContentType.ToString(),
                    content.VideoMeta != null ? content.VideoMeta.DurationSeconds : 0,
                    content.ContentUrl, content.PrewievPhotoUrl, content.Savers.Count, content.Likers.Count,
                    content.Comments.Count, content.DisLikers.Count, content.ViewsCount))
            .FirstOrDefaultAsync(cancellationToken);

        if (changedContent == null)
            return Result<ContentDto>.Failure(404, "Content not found");

        logger.LogInformation("Returned content {ContentId}",
            query.ContentId);

        return Result<ContentDto>.Success(200, changedContent);
    }
}