using Application.Contents.Create;
using Application.Searchs;
using Application.Contents.Dtos;
using Application.Channels.Dtos;
using Application.Users.Dtos;
using Microsoft.AspNetCore.Authorization;
using Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Context;
using Domain.Entities;
using API.Extensions;
using Application.Utilities;
using Application.Contents.Recommendate;
using Application.Channels;
using Application;
using Application.Contents.Search;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContentController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly ContentSearchingHandler searchingHandler;

    private readonly ILogger<ContentController> logger;
    private readonly IRecommendationService recommendation;
    private readonly IFfmpegService ffmpegService;
    private readonly IR2Service r2Service;

    public ContentController(EndlessContext context, ContentSearchingHandler searchingHandler, IRecommendationService recommendation, IFfmpegService ffmpegService, IR2Service r2Service, ILogger<ContentController> logger)
    {
        this.context = context;
        this.searchingHandler = searchingHandler;

        this.recommendation = recommendation;
        this.ffmpegService = ffmpegService;
        this.r2Service = r2Service;
        this.logger = logger;
    }

    [HttpPost("Channel/{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ContentDto>> CreateContent(Guid ChannelId, ContentCreateCommand createDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        ChannelOwner? ChannelOwner = await context.ChannelOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.ChannelId == ChannelId);

        if (ChannelOwner is null)
        {
            logger.LogWarning("User {UserId} tried to create content without permission",
               currentUserId);
            return NotFound("User not found");
        }

        if (ChannelOwner.OwnerRole <= ChannelOwnerRole.ContentEditor)
        {
            logger.LogWarning("User {UserId} tried to create content without permission",
               currentUserId);
            return Forbid("You do not have sufficient rights");
        }

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
            ChannelId = ChannelId,
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

        logger.LogInformation("content {ContentId} created in Channel {ChannelId}",
            content.Id, ChannelId);

        return Created($"api/content/{content.Id}", content.GetContentDto());
    }

    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ContentDto>> CreateContentForUser(ContentCreateCommand createDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user == null)
            return NotFound("User not found");

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

        logger.LogInformation("content {ContentId} created has user {UserId}",
            content.Id, currentUserId);

        return Created($"api/content/{content.Id}", content.GetContentDto());
    }

    [HttpGet("{ContentId}")]
    public async Task<ActionResult<ChangedContentDto>> GetChangedContent(Guid ContentId)
    {
        var changedContent = await context.Contents
            .Select(content => new
            {
                c = content,
                cResponse = new ContentDto(content.Id, content.ChannelId, content.CreatorId,
                    content.Title, content.Slug, content.Description,
                    content.CreatedDate, content.ContentType.ToString(),
                    content.VideoMeta != null ? content.VideoMeta.DurationSeconds : 0,
                    content.ContentUrl, content.PrewievPhotoUrl, content.Savers.Count, content.Likers.Count,
                    content.Comments.Count, content.DisLikers.Count, content.ViewsCount)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(content => content.c.Id == ContentId);

        if (changedContent is null || changedContent.c is null)
            return NotFound("Content not found");

        ChannelDto? ChannelResponse = await context.Channels
            .Select(Channel => new ChannelDto(
                Channel.Id, Channel.Name,
                "@" + Channel.Slug, Channel.Description ?? "",
                Channel.CreatedDate, Channel.AvatarPhotoUrl,
                Channel.Subscribers.Count, Channel.Contents.Count,
                Channel.Owners.Count, Channel.TotalLikes, Channel.TotalViews))
            .AsNoTracking()
            .FirstOrDefaultAsync(Channel => Channel.Id == changedContent.c.ChannelId);

        UserDto? userResponse = await context.Users
            .Select(user => new UserDto(
                user.Id, user.Name, "@" + user.Slug,
                user.Description ?? "", user.RegistryData, user.Email,
                user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                user.Comments.Count, user.Contents.Count, user.Followers.Count,
                user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        logger.LogInformation("Returned content {ContentId}",
            ContentId);

        return Ok(new ChangedContentDto(
            ChannelResponse, changedContent.cResponse, userResponse));
    }

    [HttpGet]
    public async Task<ActionResult<ContentRecoDto[]>> GetRandomContent()
    {
        double r = Random.Shared.NextDouble();

        var candidates = await context.Contents
            .AsNoTracking()
            .Include(c => c.VideoMeta)
            .Take(300)
            .ToArrayAsync();

        var randomContents = candidates.Select(c => new ContentRecoDto(
                c.Id, c.ChannelId, c.CreatorId, c.Title, c.Slug, c.Description,
                c.CreatedDate, c.ContentType.ToString(), Random.Shared.NextDouble(),
                c.VideoMeta == null ? 0 : c.VideoMeta.DurationSeconds, c.ContentUrl,
                c.PrewievPhotoUrl, c.Savers.Count, c.Likers.Count, c.Comments.Count,
                c.DisLikers.Count, c.ViewsCount))
            .OrderBy(c => c.RandomKey)
            .Take(25).ToArray();

        logger.LogInformation("Returned {Count} random contents",
            randomContents.Length);

        return Ok(randomContents);
    }

    [HttpGet("recommendations")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<ContentRecoDto[]>> GetContentForRecommendation()
    {
        Guid currentUserId = this.GetIDFromClaim();

        if (!await context.Users.AsNoTracking().AnyAsync(u => u.Id == currentUserId))
            return NotFound("User Not found");

        double r = Random.Shared.NextDouble();

        var candidates = await context.Contents
            .AsNoTracking()
            .Where(c => c.RandomKey > r && c.VideoMeta != null)
            .Include(c => c.VideoMeta)
            .Take(300)
            .ToListAsync();

        if (candidates.Count < 300)
        {
            var extra = await context.Contents
                .AsNoTracking()
                .Where(c => c.RandomKey < r && c.VideoMeta != null)
                .Include(c => c.VideoMeta)
                .Take(300 - candidates.Count)
                .ToListAsync();

            candidates.AddRange(extra);
        }

        UserGenreVector[] userGenres = await context.UserVectors
            .Include(uG => uG.Genre)
            .OrderBy(uG => uG.Genre!.Order)
            .Where(uG => uG.UserId == currentUserId)
            .ToArrayAsync();

        GenreInfo genreInfo = await context.GenreInfos.AsNoTracking().FirstAsync();

        IEnumerable<ContentRecoScoreQuery> recommended = candidates
                .Select(c => new ContentRecoScoreQuery(
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
            .ToArrayAsync();

        IEnumerable<Content> combined = recommended
            .Select(x => x.Content)
            .Concat(random);

        var result = combined
            .Select(c => new ContentRecoDto(
                c.Id, c.ChannelId, c.CreatorId,
                c.Title, c.Slug, c.Description,
                c.CreatedDate, c.ContentType.ToString(), Random.Shared.NextDouble(),
                c.VideoMeta == null ? 0 : c.VideoMeta.DurationSeconds, c.ContentUrl,
                c.PrewievPhotoUrl, c.Savers.Count, c.Likers.Count, c.Comments.Count,
                c.DisLikers.Count, c.ViewsCount))
            .OrderBy(x => x.RandomKey)
            .ToArray();

        logger.LogInformation("Returned {Count} recommendet contents for User {UserId}",
            result.Length, currentUserId);

        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<ContentSearchDto>> GetContentByName([FromQuery] SearchQuery query)
    {
        Result<ContentSearchDto> result = await searchingHandler.Handle(query);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                _ => StatusCode(500, "Unknown error")
            };
        }

        logger.LogInformation("Search returned Contents {Count} results for {Query}",
           result.Data.ContentsDto.Length, query.Name);

        return Ok(result.Data);
    }

    /*[HttpGet("search")]
    public async Task<ActionResult<ContentSearchDto>> GetContentForName([FromQuery] SearchQuery requestDto)
    {
        IQueryable<Content> query = context.Contents.AsQueryable();

        if (requestDto.LastSearch is not null)
        {
            query = query.Where(content =>
                EF.Functions.ILike(content.Title, $"%{requestDto.Name}%") == requestDto.LastSearch.LastLiked &&
                EF.Functions.TrigramsSimilarity(content.Title, requestDto.Name) < requestDto.LastSearch.LastSimilarity &&
                EF.Functions.FuzzyStringMatchLevenshtein(content.Title, requestDto.Name) >= requestDto.LastSearch.LastLevenshtein);
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
            .Take(20).Select(content => new
            {
                Content = new ContentDto(
                    content.Id, content.ChannelId, content.CreatorId,
                    content.Title, content.Slug, content.Description,
                    content.CreatedDate, content.ContentType.ToString(),
                    content.VideoMeta != null ? content.VideoMeta.DurationSeconds : 0,
                    content.ContentUrl, content.PrewievPhotoUrl, content.Savers.Count,
                    content.Likers.Count, content.Comments.Count, content.DisLikers.Count,
                    content.ViewsCount),
                LastLiked = EF.Functions.ILike(content.Title, $"%{requestDto.Name}%"),
                LastSimilarity = EF.Functions.TrigramsSimilarity(content.Title, requestDto.Name),
                LastLevenshtein = EF.Functions.FuzzyStringMatchLevenshtein(content.Title, requestDto.Name)
            })
            .AsNoTracking()
            .ToArrayAsync();

        var lastResponse = contents.LastOrDefault();

        ContentDto[] contentResponses = contents.Select(contnet => contnet.Content).ToArray();

        logger.LogInformation("Search returned contents {Count} results for {Query}",
           contentResponses.Length, requestDto.Name);

        return Ok(new ContentSearchDto(
            contentResponses, lastResponse == null ? null : GetSearchQuery(
                lastResponse.LastLiked, lastResponse.LastSimilarity, lastResponse.LastLevenshtein)));
    }*/

    [HttpPut("{ContentId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ContentDto>> UpdateContent(Guid ContentId, ContentCreateCommand createDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user == null)
            return NotFound("User not found");

        var content = await context.Contents
            .Where(content =>
                content.Id == ContentId &&
                content.CreatorId == currentUserId)
            .Select(content => new
            {
                c = content,
                dto = new ContentDto(
                    content.Id, content.ChannelId, content.CreatorId,
                    content.Title, content.Slug, content.Description,
                    content.CreatedDate, content.ContentType.ToString(),
                    content.VideoMeta == null ? 0 : content.VideoMeta.DurationSeconds,
                    content.ContentUrl, content.PrewievPhotoUrl, content.Savers.Count,
                    content.Likers.Count, content.Comments.Count, content.DisLikers.Count,
                    content.ViewsCount)
            })
            .FirstOrDefaultAsync();

        if (content == null)
            return NotFound("Content not found");

        VideoMetaData? videoMetaData = await context.VideoMetas
            .FirstOrDefaultAsync(videoMeta => videoMeta.ContentId == ContentId);

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

        content.c.Title = createDto.Title;
        content.c.ContentUrl = videoUrl;
        content.c.PrewievPhotoUrl = photoUrl;
        content.c.ContentType = createDto.ContentType;

        if (videoMetaData != null && videoPath != null)
        {
            videoMetaData.ContentId = ContentId;
            videoMetaData.DurationSeconds = await ffmpegService.GetVideoDuration(videoPath);

            System.IO.File.Delete(videoPath);
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Content {ContentId} updated successfully",
            ContentId);

        return Ok(content.dto);
    }

    [HttpDelete("{ContentId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> DeleteContent(Guid ContentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user is null)
            return NotFound("User not found");

        if (user.Contents.Any(c => c.Id != ContentId))
        {
            logger.LogWarning("User {UserId} tried to create content without permission",
                currentUserId);
            return Forbid("You doesn't owner the Content");
        }

        Content? content = await context.Contents
            .FirstOrDefaultAsync(c => c.Id == ContentId);

        if (content == null)
            return NotFound("Content not found");

        context.Contents.Remove(content);

        await context.SaveChangesAsync();

        logger.LogWarning("Deleted content {ContentId} for user {UserId}",
            ContentId, currentUserId);

        return NoContent();
    }
}