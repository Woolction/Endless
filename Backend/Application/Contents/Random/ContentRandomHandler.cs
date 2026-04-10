using Application.Contents.Dtos;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Contents.Random;

public class ContentRandomHandler : IRequestHandler<ContentRandomQuery, Result<ContentDto[]>>
{
    private readonly ILogger<ContentRandomHandler> logger;
    private readonly IAppDbContext context;

    public ContentRandomHandler(ILogger<ContentRandomHandler> logger, IAppDbContext context)
    {
        this.context = context;
        this.logger = logger;
    }
    
    public async Task<Result<ContentDto[]>> Handle(ContentRandomQuery query, CancellationToken cancellationToken)
    {
        double r = System.Random.Shared.NextDouble();

        var randomContents = await context.Contents
            .AsNoTracking()
            .Include(c => c.VideoMeta)
            .Where(c => c.RandomKey >= r)
            .Take(25)
            .Select(c => new ContentDto(
                c.Id, c.ChannelId, c.CreatorId, c.Title, c.Slug, c.Description,
                c.CreatedDate, c.ContentType.ToString(),
                c.VideoMeta == null ? 0 : c.VideoMeta.DurationSeconds,
                c.ContentUrl, c.PrewievPhotoUrl, c.Savers.Count, c.Likers.Count,
                c.Comments.Count, c.DisLikers.Count, c.ViewsCount))
            .ToArrayAsync(cancellationToken);

        logger.LogInformation("Returned {Count} random contents",
            randomContents.Length);

        return Result<ContentDto[]>.Success(200, randomContents);
    }
}