using Application.Features.Rows.Contents;
using Application.Features.Contents.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Imagess;
using Application.Features.Dtos;
using Application.Interfaces.Db;
using MediatR;

namespace Application.Features.Contents.Choose;

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
                    content.VideoMeta.DurationSeconds, content.VideoMeta.VideoUrl, new PhotoDto(
                        new ImageVariants(
                            content.VideoMeta.PhotoBase,
                            content.VideoMeta.Small,
                            content.VideoMeta.Medium,
                            content.VideoMeta.Large),
                        content.VideoMeta.R,
                        content.VideoMeta.G,
                        content.VideoMeta.B),
                    content.Savers.Count, content.Likers.Count, content.Comments.Count, content.DisLikers.Count, content.ViewsCount))
            .FirstOrDefaultAsync(cancellationToken);

        if (changedContent == null)
            return Result<ContentDto>.Failure(404, "Content not found");

        logger.LogInformation("Returned content {ContentId}",
            query.ContentId);

        return Result<ContentDto>.Success(200, changedContent);
    }
}