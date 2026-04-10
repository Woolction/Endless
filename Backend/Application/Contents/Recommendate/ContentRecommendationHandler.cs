using Application.Contents.Dtos;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Contents.Recommendate;

public class ContentRecommendationHandler : IRequestHandler<ContentRecommendationQuery, Result<ContentRecoDto[]>>
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
    
    public async Task<Result<ContentRecoDto[]>> Handle(ContentRecommendationQuery query, CancellationToken cancellationToken)
    {
        if (!await context.Users.AsNoTracking().AnyAsync(u => u.Id == query.UserId, cancellationToken))
            return Result<ContentRecoDto[]>.Failure(404, "User Not found");

        double r = System.Random.Shared.NextDouble();

        var candidates = await context.Contents
            .AsNoTracking()
            .Where(c => c.RandomKey > r && c.VideoMeta != null)
            .Include(c => c.VideoMeta)
            .Take(300)
            .ToListAsync(cancellationToken);

        if (candidates.Count < 300)
        {
            var extra = await context.Contents
                .AsNoTracking()
                .Where(c => c.RandomKey < r && c.VideoMeta != null)
                .Include(c => c.VideoMeta)
                .Take(300 - candidates.Count)
                .ToListAsync(cancellationToken);

            candidates.AddRange(extra);
        }

        UserGenreVector[] userGenres = await context.UserVectors
            .Include(uG => uG.Genre)
            .OrderBy(uG => uG.Genre!.Order)
            .Where(uG => uG.UserId == query.UserId)
            .ToArrayAsync(cancellationToken);

        GenreInfo genreInfo = await context.GenreInfos.AsNoTracking().FirstAsync(cancellationToken: cancellationToken);

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

        ContentRecoDto[] result = combined
            .Select(c => new ContentRecoDto(
                c.Id, c.ChannelId, c.CreatorId,
                c.Title, c.Slug, c.Description,
                c.CreatedDate, c.ContentType.ToString(), System.Random.Shared.NextDouble(),
                c.VideoMeta == null ? 0 : c.VideoMeta.DurationSeconds, c.ContentUrl,
                c.PrewievPhotoUrl, c.Savers.Count, c.Likers.Count, c.Comments.Count,
                c.DisLikers.Count, c.ViewsCount))
            .OrderBy(x => x.RandomKey)
            .ToArray();

        logger.LogInformation("Returned {Count} recommendet contents for User {UserId}",
            result.Length, query.UserId);

        return Result<ContentRecoDto[]>.Success(200, result);
    }
}