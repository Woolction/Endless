using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Backend.API.Services;
using Backend.API.Dtos;

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

    [HttpPost("domain/{DomainId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ContentResponseDto>> CreateContent(Guid DomainId, ContentCreateDto createDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        DomainOwner? domainOwner = await context.DomainOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.DomainId == DomainId);

        if (domainOwner is null)
            return BadRequest("User not found");

        if (domainOwner.OwnerRole <= DomainOwnerRole.ContentEditor)
            return BadRequest("You do not have sufficient rights");

        string videoUrl = null!;
        string videoPath = null!;

        if (createDto.ContentFile != null && createDto.ContentFile.Length != 0)
        {
            videoPath = await r2Service.SaveFormFileAsync(createDto.ContentFile, "Video");
            videoUrl = await ffmpegService.UploadGeneratedVideos(videoPath);
        }

        string photoUrl = null!;

        if (createDto.PrewievPhoto != null && createDto.PrewievPhoto.Length != 0)
        {
            string photoPath = await r2Service.SaveFormFileAsync(createDto.PrewievPhoto, "Images", ".jpeg");
            photoUrl = await r2Service.SaveImage(photoPath);

            System.IO.File.Delete(photoPath);
        }

        Content content = new()
        {
            CreatorId = currentUserId,
            DomainId = DomainId,
            Title = createDto.Title,
            Slug = Guid.NewGuid(),
            ContentUrl = videoUrl,
            PrewievPhotoUrl = photoUrl,
            CreatedDate = DateTime.UtcNow,
            RandomKey = Random.Shared.NextDouble(),
            ContentType = createDto.ContentType
        };

        if (videoPath != null)
        {
            VideoMetaData metaData = new()
            {
                Content = content,
                DurationSeconds = await ffmpegService.GetVideoDuration(videoPath)
            };

            System.IO.File.Delete(videoPath);

            context.VideoMetas.Add(metaData);
        }

        context.Contents.Add(content);
        context.ContentVectors.AddRange(await context.Genres
            .Select(genre => new ContentGenreVector()
            {
                Content = content,
                GenreId = genre.Id
            })
            .AsNoTracking()
            .ToListAsync()
        );

        await context.SaveChangesAsync();

        return Ok(content.GetContentResponseDto());
    }

    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ContentResponseDto>> CreateContentForUser(ContentCreateDto createDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user == null)
            return BadRequest("User not found");

        string videoUrl = null!;
        string videoPath = null!;

        if (createDto.ContentFile != null && createDto.ContentFile.Length != 0)
        {
            videoPath = await r2Service.SaveFormFileAsync(createDto.ContentFile, "Video");
            videoUrl = await ffmpegService.UploadGeneratedVideos(videoPath);
        }

        string photoUrl = null!;

        if (createDto.PrewievPhoto != null && createDto.PrewievPhoto.Length != 0)
        {
            string photoPath = await r2Service.SaveFormFileAsync(createDto.PrewievPhoto, "Images", ".jpeg");
            photoUrl = await r2Service.SaveImage(photoPath);

            System.IO.File.Delete(photoPath);
        }

        Content content = new()
        {
            CreatorId = currentUserId,
            Title = createDto.Title,
            Slug = Guid.NewGuid(),
            ContentUrl = videoUrl,
            PrewievPhotoUrl = photoUrl,
            CreatedDate = DateTime.UtcNow,
            RandomKey = Random.Shared.NextDouble(),
            ContentType = createDto.ContentType
        };

        if (videoPath != null)
        {
            VideoMetaData metaData = new()
            {
                Content = content,
                DurationSeconds = await ffmpegService.GetVideoDuration(videoPath)
            };

            System.IO.File.Delete(videoPath);

            context.VideoMetas.Add(metaData);
        }

        context.Contents.Add(content);
        context.ContentVectors.AddRange(await context.Genres
            .Select(genre => new ContentGenreVector()
            {
                Content = content,
                GenreId = genre.Id
            })
            .AsNoTracking()
            .ToListAsync()
        );

        await context.SaveChangesAsync();

        return Ok(content.GetContentResponseDto()); //Test
    }

    [HttpGet("{ContentId}")]
    public async Task<ActionResult<ChangedContentDto>> GetChangedContent(Guid ContentId)
    {
        var changedContent = await context.Contents
            .Select(content => new {
                c = content,
                cResponse = new ContentResponseDto(content.Id, content.DomainId, content.CreatorId,
                    content.Title, content.Slug, content.Description,
                    content.CreatedDate, content.ContentType.ToString(),
                    content.VideoMeta != null ? content.VideoMeta.DurationSeconds : 0,
                    content.ContentUrl, content.PrewievPhotoUrl, content.Savers.Count, content.Likers.Count,
                    content.Comments.Count, content.DizLikers.Count, content.ViewsCount)})
            .AsNoTracking()
            .FirstOrDefaultAsync(content => content.c.Id == ContentId);

        if (changedContent is null || changedContent.c is null)
            return BadRequest("Content not found");

        DomainResponseDto? domainResponse = await context.Domains
            .Select(domain => new DomainResponseDto(
                domain.Id, domain.Name,
                "@" + domain.Slug, domain.Description ?? "",
                domain.CreatedDate, domain.AvatarPhotoUrl,
                domain.Subscribers.Count, domain.Contents.Count,
                domain.Owners.Count, domain.TotalLikes, domain.TotalViews))
            .AsNoTracking()
            .FirstOrDefaultAsync(domain => domain.Id == changedContent.c.DomainId);

        UserResponseDto? userResponse = await context.Users
            .Select(user => new UserResponseDto(
                user.Id, user.Name, "@" + user.Slug,
                user.Description ?? "", user.RegistryData, user.Email,
                user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                user.Comments.Count, user.Contents.Count, user.Followers.Count,
                user.Following.Count, user.OwnedDomains.Count, user.SubscripedDomains.Count))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return Ok(new ChangedContentDto(
            domainResponse, changedContent.cResponse, userResponse));
    }

    [HttpGet]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<ContentRecoResponseDto>> GetContentForRecommendation()
    {
        Guid currentUserId = this.GetIDFromClaim();

        if (!await context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == currentUserId))
            return BadRequest("User Not found");

        double r = Random.Shared.NextDouble();

        var candidates = await context.Contents
            .AsNoTracking()
            .Where(c => c.RandomKey >= r && c.VideoMeta != null)
            .Include(c => c.Vectors)
            .Include(c => c.VideoMeta)
            .OrderBy(c => c.RandomKey)
            .Take(300).ToListAsync();

        if (candidates.Count < 300)
        {
            var extra = await context.Contents
                .AsNoTracking()
                .Where(c => c.RandomKey < r && c.VideoMeta != null)
                .Include(c => c.Vectors)
                .Include(c => c.VideoMeta)
                .OrderBy(c => c.RandomKey)
                .Take(300 - candidates.Count)
                .ToListAsync();

            candidates.AddRange(extra);
        }

        ContentRecoScoreDto[] recommended = [];

        UserGenreVector[] userGenres = await context.UserVectors
            .Include(uG => uG.Genre)
            .OrderBy(uG => uG.Genre!.Order)
            .Where(uG => uG.UserId == currentUserId)
            .ToArrayAsync();

        GenreInfo genreInfo = await context.GenreInfos.AsNoTracking().FirstAsync();

        recommended = candidates
                .Select(c => new ContentRecoScoreDto(
                    c, recommendation.Recommend(userGenres, c, c.VideoMeta!, context.ContentVectors
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
            .Select(x => new ContentRecoDto(
                x.Content.Id, x.Content.DomainId, x.Content.CreatorId,
                x.Content.Title, x.Content.Slug, x.Content.Description,
                x.Content.CreatedDate, x.Content.ContentType.ToString(), Random.Shared.NextDouble(),
                x.Content.VideoMeta?.DurationSeconds, x.Content.ContentUrl, x.Content.PrewievPhotoUrl,
                x.Content.Savers.Count, x.Content.Likers.Count, x.Content.Comments.Count,
                x.Content.DizLikers.Count, x.Content.ViewsCount))
            .Concat(random.Select(content => new ContentRecoDto(
                content.Id, content.DomainId, content.CreatorId,
                content.Title, content.Slug, content.Description,
                content.CreatedDate, content.ContentType.ToString(), Random.Shared.NextDouble(),
                content.VideoMeta?.DurationSeconds, content.ContentUrl, content.PrewievPhotoUrl,
                content.Savers.Count, content.Likers.Count, content.Comments.Count,
                content.DizLikers.Count, content.ViewsCount)))
            .OrderBy(x => x.RandomKey).ToArray();

        return Ok(new ContentRecoResponseDto(combined));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ContentSearchResponseDto>> GetContentForName([FromQuery] SearchRequestDto requestDto)
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

        var contents = await query
            .OrderByDescending(content => EF.Functions.TrigramsSimilarity(content.Title, requestDto.Name))
            .Take(20).Select(content => new {
                Content = new ContentResponseDto(
                    content.Id, content.DomainId, content.CreatorId,
                    content.Title, content.Slug, content.Description,
                    content.CreatedDate, content.ContentType.ToString(),
                    content.VideoMeta != null ? content.VideoMeta.DurationSeconds : 0,
                    content.ContentUrl, content.PrewievPhotoUrl, content.Savers.Count,
                    content.Likers.Count, content.Comments.Count, content.DizLikers.Count,
                    content.ViewsCount),
                LastLiked = EF.Functions.ILike(content.Title, $"%{requestDto.Name}%"),
                LastSimilarity = EF.Functions.TrigramsSimilarity(content.Title, requestDto.Name),
                LastLevenshit = EF.Functions.FuzzyStringMatchLevenshtein(content.Title, requestDto.Name)
            }).AsNoTracking().ToArrayAsync();

        var lastResponse = contents.LastOrDefault();

        ContentResponseDto[] contentResponses = contents.Select(contnet => contnet.Content).ToArray();

        return Ok(new ContentSearchResponseDto(
            contentResponses, lastResponse == null ? null : GetSearchDto(
                lastResponse.LastLiked, lastResponse.LastSimilarity, lastResponse.LastLevenshit)));
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
            .FirstOrDefaultAsync(c => c.Id == ContentId);

        if (content == null)
            return BadRequest("Content not found");

        context.Contents.Remove(content);

        await context.SaveChangesAsync();

        return NoContent();
    }

    private SearchDto GetSearchDto(bool IsLastLiked, double LastSimilarity, int LastLevenshit)
    {
        return new()
        {
            LastLiked = IsLastLiked,
            LastSimilarity = LastSimilarity,
            LastLevenshit = LastLevenshit
        };
    }
}