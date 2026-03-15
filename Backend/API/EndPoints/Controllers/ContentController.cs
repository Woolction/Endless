using System.Runtime.InteropServices;
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

    [HttpPost("{DomainId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> CreateContent(Guid DomainId, ContentCreateDto createDto)
    {
        if (createDto.ContentFile == null || createDto.ContentFile.Length == 0)
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

        string videoPath = await r2Service.SaveFormFileAsync(createDto.ContentFile, "Video");
        string videoUrl = await ffmpegService.UploadGeneratedVideos(videoPath);

        string photoUrl = null!;

        if (createDto.PrewievPhoto is not null)
        {
            string photoPath = await r2Service.SaveFormFileAsync(createDto.PrewievPhoto, "Images", ".jpeg");
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
            RandomKey = Random.Shared.NextDouble(),
            ContentType = createDto.ContentType
        };

        VideoMetaData metaData = new()
        {
            Content = content,
            DurationSeconds = await ffmpegService.GetVideoDuration(videoPath)
        };

        System.IO.File.Delete(videoPath);

        context.Contents.Add(content);
        context.VideoMetas.Add(metaData);
        context.ContentVectors.AddRange(await context.Genres
            .Select(genre => new ContentGenreVector() {
                Content = content,
                Genre = genre })
            .AsNoTracking()
            .ToListAsync()
        );

        await context.SaveChangesAsync();

        return Ok(content.GetContentResponseDto());
    }

    [HttpGet("{ContentId}")]
    public async Task<IActionResult> GetChangedContent(Guid ContentId)
    {
        Content? changedContent = await context.Contents
            .AsNoTracking()
            .Include(content => content.VideoMeta)
            .Include(content => content.Creator)
            .Include(content => content.Domain)
            .FirstOrDefaultAsync(content => content.Id == ContentId);

        if (changedContent is null)
            return BadRequest("Content not found");

        DomainResponseDto domainResponseDto = changedContent.Domain!.GetDomainResponseDto();
        ContentResponseDto contentResponseDto = changedContent.GetContentResponseDto();
        UserResponseDto userResponseDto = changedContent.Creator!.GetUserResponseDto();

        return Ok(new { domainResponseDto, contentResponseDto, userResponseDto });
    }

    [HttpGet]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> GetContentForRecommendation()//ContentSearchRequestDto requestDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        if (!await context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == currentUserId))
            return BadRequest("User Not found");

        double r = Random.Shared.NextDouble();

        var candidates = await context.Contents
            .AsNoTracking()
            .Where(c => c.RandomKey >= r)
            .Include(c => c.Vectors)
            .Include(c => c.VideoMeta)
            .OrderBy(c => c.RandomKey)
            .Take(300).ToListAsync();

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

        UserGenreVector[] userGenres = await context.UserVectors
            .Include(uG => uG.Genre)
            .OrderBy(uG => uG.Genre!.Order)
            .Where(uG => uG.UserId == currentUserId)
            .ToArrayAsync();

        GenreInfo genreInfo = await context.GenreInfos.AsNoTracking().FirstAsync();

        recommended = candidates
                .Select(c => new ContentRecoScoreDto(
                    c, recommendation.Recommend(userGenres, c, context.ContentVectors
                        .Include(cG => cG.Genre)
                        .OrderBy(cG => cG.Genre!.Order)
                        .Where(cG => cG.ContentId == c.Id)
                        .ToArray(), genreInfo.Count)))
                .OrderByDescending(x => x.Score)
                .Take(16).ToArray();

        var recommendedIds = recommended.Select(x => x.Content.Id).ToHashSet();

        var random = await context.Contents
            .Where(x => !recommendedIds.Contains(x.Id))
            .OrderBy(x => x.RandomKey >= r)
            .Take(4).ToListAsync();

        var combined = recommended
            .Select(x => x.Content.GetContentRecoDto())
            .Concat(random.Select(content => content.GetContentRecoDto()))
            .OrderBy(x => x.RandomKey).ToArray();

        return Ok(new ContentRecoResponseDto(combined)); //, lastScore));
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetContentForName([FromQuery] SearchRequestDto requestDto)
    {
        IQueryable<Content> query = context.Contents.AsQueryable();

        if (requestDto.LastSearch is not null)
        {
            query = query.Where(content =>
                EF.Functions.ILike(content.Title, $"%{requestDto.Name}%") == requestDto.LastSearch.LastLiked &&
                EF.Functions.TrigramsSimilarity(content.Title, requestDto.Name) < requestDto.LastSearch.LastSimilarity &&
                EF.Functions.FuzzyStringMatchLevenshtein(content.Title, requestDto.Name) >= requestDto.LastSearch.LastLevenshit);
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
            .Take(20).Select(content => content.GetContentResponseDto())
            .AsNoTracking().ToArrayAsync();

        ContentResponseDto? lastContent = contents.LastOrDefault();
      
        return Ok(new ContentSearchResponseDto(contents, GetSearchDto(lastContent, requestDto)));
    }

    [HttpPut("{ContentId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> UpdateContent(Guid ContentId)
    {
        return Ok();
    }

    [HttpDelete("{ContentId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> DeleteContent(Guid ContentId)
    {
        Guid UserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(UserId);

        if (user is null)
            return BadRequest("User not found");

        if (user.Contents.Any(c => c.Id != ContentId))
            return Forbid("You doesn't owner the Content");

        Content? content = await context.Contents
            .Include(c => c.Domain)
            .FirstOrDefaultAsync(c => c.Id == ContentId);

        if (content is not null)
        {
            user.ContentsCount--;

            content.Domain!.ContentsCount--;

            context.Contents.Remove(content);

            await context.SaveChangesAsync();
        }

        return NoContent();
    }

    private SearchDto? GetSearchDto(ContentResponseDto? content, SearchRequestDto requestDto)
    {
        if (content is null)
            return null;

        return new()
        {
            LastLiked = EF.Functions.ILike(content.Title, $"%{requestDto.Name}%"),
            LastSimilarity = EF.Functions.TrigramsSimilarity(content.Title, requestDto.Name),
            LastLevenshit = EF.Functions.FuzzyStringMatchLevenshtein(content.Title, requestDto.Name)
        };
    }
}