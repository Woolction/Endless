using Application.Features.Contents.Dtos;
using Application.Features.Rows.Contents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Imagess;
using Application.Interfaces.Db;
using Application.Features.Dtos;
using MediatR;

namespace Application.Features.Contents.Random;

public class ContentRandomHandler : IRequestHandler<ContentRandomQuery, Result<ContentFeedDto[]>>
{
    private readonly ILogger<ContentRandomHandler> logger;
    private readonly IAppDbContext context;

    public ContentRandomHandler(ILogger<ContentRandomHandler> logger, IAppDbContext context)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<Result<ContentFeedDto[]>> Handle(ContentRandomQuery query, CancellationToken cancellationToken)
    {
        double r = System.Random.Shared.NextDouble();

        var randomContents = await context.Contents
            .AsNoTracking()
            .Where(c => c.RandomKey >= r)
            .Select(c => new ContentFeedDto(
                c.Id, c.ChannelId, c.CreatorId,
                c.Channel == null ? c.Creator!.Name : c.Channel.Name,
                c.Channel == null ? c.Creator!.Slug : c.Channel.Slug,
                c.Channel == null ?
                new PhotoDto(
                    new ImageVariants(
                        c.Creator!.UserMeta.IconBase,
                        c.Creator!.UserMeta.Small,
                        c.Creator!.UserMeta.Medium,
                        c.Creator!.UserMeta.Large),
                    c.Creator!.UserMeta.R,
                    c.Creator!.UserMeta.G,
                    c.Creator!.UserMeta.B) :
                new PhotoDto(
                    new ImageVariants(
                        c.Channel.ChannelMeta.IconBase,
                        c.Channel.ChannelMeta.Small,
                        c.Channel.ChannelMeta.Medium,
                        c.Channel.ChannelMeta.Large),
                    c.Channel.ChannelMeta.R,
                    c.Channel.ChannelMeta.G,
                    c.Channel.ChannelMeta.B),
                c.Title, c.Slug, c.Description, c.CreatedDate, c.ContentType.ToString(),
                c.VideoMeta.DurationSeconds, c.VideoMeta.VideoUrl, new PhotoDto(
                    new ImageVariants(
                        c.VideoMeta.PhotoBase,
                        c.VideoMeta.Small,
                        c.VideoMeta.Medium,
                        c.VideoMeta.Large),
                    c.VideoMeta.R,
                    c.VideoMeta.G,
                    c.VideoMeta.B),
                c.ViewsCount))
            .Take(25)
            .ToArrayAsync(cancellationToken);

        logger.LogInformation("Returned {Count} random contents",
            randomContents.Length);

        return Result<ContentFeedDto[]>.Success(200, randomContents);
    }


}