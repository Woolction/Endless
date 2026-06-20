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
            .Include(c => c.Meta)
            .Include(c => c.Channel)
                .ThenInclude(ch => ch.Meta)
            .Include(c => c.Creator)
                .ThenInclude(u => u.Meta)
            .Take(300)
            .ToListAsync(cancellationToken);

        if (candidates.Count < 300)
        {
            var extra = await context.Contents
                .AsNoTracking()
                .Where(c => c.RandomKey < r && c.Meta != null)
                .Include(c => c.Meta)
                    .ThenInclude(m => m.Image)
                        .ThenInclude(i => i.Variants)
                .Include(c => c.Channel)
                    .ThenInclude(ch => ch.Meta)
                        .ThenInclude(m => m.Image)
                            .ThenInclude(i => i.Variants)
                .Include(c => c.Creator)
                    .ThenInclude(u => u.Meta)
                        .ThenInclude(m => m.Image)
                            .ThenInclude(i => i.Variants)
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
                c, recommendation.Recommend(userGenres, c, c.Meta, context.ContentVectors
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
                        c.Creator!.Meta.Image.BaseUrl,
                        c.Creator.Meta.Image.Variants
                            .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                            .ToList(),
                        c.Creator.Meta.Image.R,
                        c.Creator.Meta.Image.G,
                        c.Creator.Meta.Image.B) :
                    new ImageDto(
                        c.Channel.Meta.Image.BaseUrl,
                        c.Channel.Meta.Image.Variants
                            .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                            .ToList(),
                        c.Channel.Meta.Image.R,
                        c.Channel.Meta.Image.G,
                        c.Channel.Meta.Image.B);

                return new ContentFeedDto(
                    c.Id, c.ChannelId, c.CreatorId,
                    owner.Name, owner.Slug, imageDto,
                    c.Title, c.Slug, c.Description, c.CreatedDate, c.ContentType.ToString(),
                    c.Meta.DurationSeconds, c.Meta.VideoUrl, new ImageDto(
                        c.Meta.Image.BaseUrl,
                        c.Meta.Image.Variants
                            .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                            .ToList(),
                        c.Meta.Image.R,
                        c.Meta.Image.G,
                        c.Meta.Image.B),
                    c.ViewsCount);
            })
            .OrderBy(_ => System.Random.Shared.NextDouble())
            .ToArray();

        logger.LogInformation("Returned {Count} recommendet contents for User {UserId}",
            result.Length, query.UserId);

        return Result<ContentFeedDto[]>.Success(200, result);
    }
}