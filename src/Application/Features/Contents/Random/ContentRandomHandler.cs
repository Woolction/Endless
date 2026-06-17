using Application.Features.Contents.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Images;
using Application.Interfaces.Db;
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
                new ImageDto(
                    new ImageVariantsDto(
                        c.Creator!.UserMeta.Image.BaseUrl,
                        c.Creator!.UserMeta.Image.Variants
                            .Select(v => new ImageVariantDto(v.Url, v.Height, v.Width))
                            .ToList()),
                    c.Creator!.UserMeta.Image.R,
                    c.Creator!.UserMeta.Image.G,
                    c.Creator!.UserMeta.Image.B) :
                new ImageDto(
                    new ImageVariantsDto(
                        c.Channel.ChannelMeta.Image.BaseUrl,
                        c.Channel.ChannelMeta.Image.Variants
                            .Select(v => new ImageVariantDto(v.Url, v.Height, v.Width))
                            .ToList()),
                    c.Channel.ChannelMeta.Image.R,
                    c.Channel.ChannelMeta.Image.G,
                    c.Channel.ChannelMeta.Image.B),
                c.Title, c.Slug, c.Description, c.CreatedDate, c.ContentType.ToString(),
                c.VideoMeta.DurationSeconds, c.VideoMeta.VideoUrl, new ImageDto(
                    new ImageVariantsDto(
                        c.VideoMeta.Image.BaseUrl,
                        c.VideoMeta.Image.Variants
                            .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                            .ToList()),
                    c.VideoMeta.Image.R,
                    c.VideoMeta.Image.G,
                    c.VideoMeta.Image.B),
                c.ViewsCount))
            .Take(25)
            .ToArrayAsync(cancellationToken);

        logger.LogInformation("Returned {Count} random contents",
            randomContents.Length);

        return Result<ContentFeedDto[]>.Success(200, randomContents);
    }


}