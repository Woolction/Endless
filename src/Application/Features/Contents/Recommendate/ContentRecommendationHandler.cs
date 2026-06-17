using Application.Features.Contents.Dtos;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Images;
using Application.Interfaces.Db;
using Domain.Entities;
using MediatR;

namespace Application.Features.Contents.Recommendate;

public class ContentRecommendationHandler : IRequestHandler<ContentRecommendationQuery, Result<ContentFeedDto[]>>
{
    private readonly ILogger<ContentRecommendationHandler> logger;
    private readonly IRecommendationService recommendation;
    private readonly IAppDbContext context;
    public ContentRecommendationHandler(IAppDbContext context, IRecommendationService recommendation, ILogger<ContentRecommendationHandler> logger)
    {
        this.context = context;

        this.recommendation = recommendation;
        this.logger = logger;
    }

    public async Task<Result<ContentFeedDto[]>> Handle(ContentRecommendationQuery query, CancellationToken cancellationToken)
    {
        if (!await context.Users.AsNoTracking().AnyAsync(u => u.Id == query.UserId, cancellationToken))
            return Result<ContentFeedDto[]>.Failure(404, "User Not found");

        double r = System.Random.Shared.NextDouble();

        var candidates = await context.Contents
            .AsNoTracking()
            .Where(c => c.RandomKey > r)
            .Include(c => c.VideoMeta)
            .Include(c => c.Channel)
                .ThenInclude(ch => ch.ChannelMeta)
            .Include(c => c.Creator)
                .ThenInclude(u => u.UserMeta)
            .Take(300)
            .ToListAsync(cancellationToken);

        if (candidates.Count < 300)
        {
            var extra = await context.Contents
                .AsNoTracking()
                .Where(c => c.RandomKey < r && c.VideoMeta != null)
                .Include(c => c.VideoMeta)
                .Include(c => c.Channel)
                    .ThenInclude(ch => ch.ChannelMeta)
                .Include(c => c.Creator)
                    .ThenInclude(u => u.UserMeta)
                .Take(300 - candidates.Count)
                .ToListAsync(cancellationToken);

            candidates.AddRange(extra);
        }

        UserGenreVector[] userGenres = await context.UserVectors
            .Include(uG => uG.Genre)
            .OrderBy(uG => uG.Genre!.Order)
            .Where(uG => uG.UserId == query.UserId)
            .ToArrayAsync(cancellationToken);

        GenreInfo genreInfo = await context.GenreInfos
            .AsNoTracking()
            .FirstAsync(cancellationToken: cancellationToken);

        IEnumerable<ContentRecoScore> recommended = candidates
            .Select(c => new ContentRecoScore(
                c, recommendation.Recommend(userGenres, c, c.VideoMeta, context.ContentVectors
                    .Include(cG => cG.Genre)
                    .OrderBy(cG => cG.Genre!.Order)
                    .Where(cG => cG.ContentId == c.Id)
                    .ToArray(), genreInfo.Count)))
            .OrderByDescending(x => x.Score)
            .Take(20);

        var recommendedIds = recommended.Select(x => x.Content.Id).ToHashSet();

        var random = await context.Contents
            .Where(x => !recommendedIds.Contains(x.Id))
            .Take(5)
            .ToArrayAsync(cancellationToken: cancellationToken);

        IEnumerable<Content> combined = recommended
            .Select(x => x.Content)
            .Concat(random);

        ContentFeedDto[] result = combined
            .Select(c =>
            {
                dynamic owner = (c.Channel == null ? c.Creator : c.Channel)!;

                ImageDto imageDto = c.Channel == null ?
                    new ImageDto(
                        new ImageVariantsDto(
                            c.Creator!.UserMeta.Image.BaseUrl,
                            c.Creator.UserMeta.Image.Variants
                                .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                                .ToList()),
                        c.Creator.UserMeta.Image.R,
                        c.Creator.UserMeta.Image.G,
                        c.Creator.UserMeta.Image.B) :
                    new ImageDto(
                        new ImageVariantsDto(
                            c.Channel.ChannelMeta.Image.BaseUrl,
                            c.Channel.ChannelMeta.Image.Variants
                                .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                                .ToList()),
                        c.Channel.ChannelMeta.Image.R,
                        c.Channel.ChannelMeta.Image.G,
                        c.Channel.ChannelMeta.Image.B);

                return new ContentFeedDto(
                    c.Id, c.ChannelId, c.CreatorId,
                    owner.Name, owner.Slug, imageDto,
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
                    c.ViewsCount);
            })
            .OrderBy(_ => System.Random.Shared.NextDouble())
            .ToArray();

        logger.LogInformation("Returned {Count} recommendet contents for User {UserId}",
            result.Length, query.UserId);

        return Result<ContentFeedDto[]>.Success(200, result);
    }
}