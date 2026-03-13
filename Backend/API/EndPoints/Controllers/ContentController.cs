using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Dtos;
using Backend.API.Extensions;
using Backend.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.EndPoints.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContentController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly IRecommendationService recommendation;
    private readonly IFfmpegService ffmpegService;
    private readonly IR2Service r2Service;

    public ContentController(EndlessContext context, IRecommendationService recommendation, IFfmpegService ffmpegService, IR2Service r2Service)
    {
        this.context = context;

        this.recommendation = recommendation;
        this.ffmpegService = ffmpegService;
        this.r2Service = r2Service;
    }

    [HttpGet]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> GetContentForRecommendation()//ContentSearchRequestDto requestDto)
    {
        Guid id = this.GetIDFromClaim();

        User? currentUser = await context.Users
            .Include(u => u.Vectors)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (currentUser is null)
            return BadRequest("User Not found");

        double r = Random.Shared.NextDouble();

        var candidates = await context.Contents
            .AsNoTracking()
            .Where(c => c.RandomKey >= r)
            .Include(c => c.Vectors)
            .Include(c => c.VideoMeta)
            .OrderBy(c => c.RandomKey)
            .Take(300)
            .ToListAsync();

        if (candidates.Count < 300)
        {
            var extra = await context.Contents
                .AsNoTracking()
                .Where(c => c.RandomKey < r)
                .Include(c => c.Vectors)
                .Include(c => c.VideoMeta)
                .OrderBy(c => c.RandomKey)
                .Take(300 - candidates.Count)
                .ToListAsync();

            candidates.AddRange(extra);
        }

        ContentRecoScoreDto[] recommended = [];

        /*if (requestDto.LastRecommendScore is not null)
        {
            recommended = candidates
                .Select(c => new ContentRecoScoreDto(
                    c, recommendation.Recommend(currentUser, c)))
                .Where(c =>
                   c.Score < requestDto.LastRecommendScore)
                .OrderByDescending(x => x.Score)
                .Take(16)
                .ToArray();   
        } else {}
        var LastRecommended = recommended.LastOrDefault();
        float? lastScore = null;

        if (LastRecommended is not null)
            lastScore = LastRecommended.Score;*/

        recommended = candidates
                .Select(c => new ContentRecoScoreDto(
                    c, recommendation.Recommend(currentUser, c)))
                .OrderByDescending(x => x.Score)
                .Take(16)
                .ToArray();

        var recommendedIds = recommended.Select(x => x.Content.Id).ToHashSet();

        var random = await context.Contents
            .Where(x => !recommendedIds.Contains(x.Id))
            .OrderBy(x => x.RandomKey >= r)
            .Take(4)
            .ToListAsync();

        var combined = recommended
            .Select(x => new ContentRecoDto(
                x.Content.Id, x.Content.DomainId, x.Content.CreatorId,
                x.Content.Title, x.Content.Slug, x.Content.Description,
                x.Content.CreatedDate, x.Content.ContentType, Random.Shared.NextDouble(),
                x.Content.VideoMeta, x.Content.ContentUrl, x.Content.PrewievPhotoUrl,
                x.Content.SavesCount, x.Content.LikesCount, x.Content.CommentsCount,
                x.Content.DizLikesCount, x.Content.ViewsCount))
            .Concat(random.Select(content => new ContentRecoDto(
                content.Id, content.DomainId, content.CreatorId,
                content.Title, content.Slug, content.Description,
                content.CreatedDate, content.ContentType, Random.Shared.NextDouble(),
                content.VideoMeta, content.ContentUrl, content.PrewievPhotoUrl,
                content.SavesCount, content.LikesCount, content.CommentsCount,
                content.DizLikesCount, content.ViewsCount)))
            .OrderBy(x => x.RandomKey).ToArray();

        return Ok(new ContentRecoResponseDto(combined)); //, lastScore));
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetContentForName(SearchRequestDto requestDto)
    {
        IQueryable<Content> query = context.Contents.AsQueryable();

        if (requestDto.LastSimilarity is not null)
        {
            query = query.Where(content =>
                EF.Functions.TrigramsSimilarity(content.Title, requestDto.Name) < requestDto.LastSimilarity);
        }
        else
        {
            query = query.Where(content =>
                EF.Functions.ILike(content.Title, $"%{requestDto.Name}%") ||
                EF.Functions.TrigramsSimilarity(content.Title, requestDto.Name) > 0.2f ||
                EF.Functions.FuzzyStringMatchLevenshtein(content.Title, requestDto.Name) <= 3);
        }

        ContentResponseDto[] contents = await query
            .OrderByDescending(content => EF.Functions.TrigramsSimilarity(content.Title, requestDto.Name))
            .Take(20)
            .Select(content => new ContentResponseDto(
                content.Id, content.DomainId, content.CreatorId,
                content.Title, content.Slug, content.Description,
                content.CreatedDate, content.ContentType,
                content.VideoMeta, content.ContentUrl, content.PrewievPhotoUrl,
                content.SavesCount, content.LikesCount, content.CommentsCount,
                content.DizLikesCount, content.ViewsCount))
            .AsNoTracking().ToArrayAsync();

        ContentResponseDto? lastDomain = contents.LastOrDefault();
        double? lastSimiratity = null;

        if (lastDomain is not null)
            lastSimiratity = EF.Functions.TrigramsSimilarity(lastDomain.Title, requestDto.Name);

        return Ok(new ContentSearchResponseDto(contents, lastSimiratity));
    }

    [HttpPost("{DomainId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> CreateContent(IFormFile contentFile, IFormFile? prewievPhoto, Guid DomainId, ContentCreateDto createDto)
    {
        if (contentFile == null || contentFile.Length == 0)
            return BadRequest("Empty contentFile");

        Guid currentUserId = this.GetIDFromClaim();

        Domain? domain = await context.Domains
            .AsNoTracking()
            .FirstOrDefaultAsync(domain =>
                domain.Id == DomainId);

        if (domain is null)
            return BadRequest("Domain not found");

        DomainOwner? domainOwner = await context.DomainOwners
            .AsNoTracking()
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.DomainId == domain.Id);

        if (domainOwner is null)
            return BadRequest("User not found");

        if (domainOwner.OwnerRole <= DomainOwnerRole.ContentEditor)
            return BadRequest("You do not have sufficient rights");

        string videoPath = await r2Service.SaveFormFileAsync(contentFile, "Video");
        string videoUrl = await ffmpegService.UploadGeneratedVideos(videoPath);

        string photoUrl = null!;

        if (prewievPhoto is not null)
        {
            string photoPath = await r2Service.SaveFormFileAsync(prewievPhoto, "Images", ".jpeg");
            photoUrl = await r2Service.SaveImage(photoPath);

            System.IO.File.Delete(photoPath);
        }

        Content content = new()
        {
            CreatorId = currentUserId,
            DomainId = domain.Id,
            Title = createDto.Title,
            Slug = Guid.NewGuid(),
            ContentUrl = videoUrl,
            PrewievPhotoUrl = photoUrl,
            CreatedDate = DateTime.UtcNow,
            RandomKey = Random.Shared.NextDouble()
        };
        content.Vectors.AddRange(await context.ContentVectors.ToListAsync());
        content.VectorsCount = content.Vectors.Count;

        VideoMetaData metaData = new()
        {
            Content = content,
            DurationSeconds = await ffmpegService.GetVideoDuration(videoPath)
        };

        System.IO.File.Delete(videoPath);

        context.Contents.Add(content);
        context.VideoMetas.Add(metaData);

        await context.SaveChangesAsync();

        ContentResponseDto responseDto = new (
            content.Id, content.DomainId, content.CreatorId,
            content.Title, content.Slug, content.Description,
            content.CreatedDate, content.ContentType,
            metaData, content.ContentUrl, content.PrewievPhotoUrl,
            content.SavesCount, content.LikesCount, content.CommentsCount,
            content.DizLikesCount, content.ViewsCount);

        return Ok(responseDto);
    }

    [HttpPut]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> UpdateContent()
    {
        return Ok();
    }

    [HttpDelete]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> DeleteContent()
    {
        return NoContent();
    }
}