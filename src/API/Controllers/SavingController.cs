using Microsoft.AspNetCore.Authorization;
using Application.Features.Contents.Dtos;
using Microsoft.EntityFrameworkCore;
using Application.Features.Images;
using Application.Interfaces.Db;
using Microsoft.AspNetCore.Mvc;
using Application.Utilities;
using Domain.Common.Enums;
using Domain.Entities;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SavingController : ControllerBase
{
    private readonly IAppDbContext context;

    private readonly ILogger<SavingController> logger;

    public SavingController(IAppDbContext context, ILogger<SavingController> logger)
    {
        this.context = context;

        this.logger = logger;
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<ContentDto>> SaveContent(Guid ContentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        var content = await context.Contents
            .Where(content => content.Id == ContentId)
            .Select(content => new ContentDto(
                    content.Id, content.ChannelId, content.CreatorId,
                    content.Title, content.Slug, content.Description,
                    content.CreatedDate, content.ContentType.ToString(),
                    content.Meta.DurationSeconds, content.Meta.VideoUrl,
                    new ImageDto(
                        content.Meta.Image.BaseUrl,
                        content.Meta.Image.Variants
                            .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                            .ToList(),
                        content.Meta.Image.R,
                        content.Meta.Image.G,
                        content.Meta.Image.B),
                    content.Savers.Count + 1, content.Likers.Count, content.Comments.Count,
                    content.DisLikers.Count, content.ViewsCount))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (currentUser is null)
            return NotFound("User not found");
        if (content is null)
            return NotFound("Content not found");

        SavedContent savedContent = new()
        {
            UserId = currentUserId,
            ContentId = ContentId,
            SavedDate = DateTime.UtcNow
        };

        context.SavedContents.Add(savedContent);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} saved content {ContentId}",
          currentUserId, ContentId);

        return Created($"api/saving/user/{savedContent.UserId}/content/{savedContent.ContentId}",
            content);
    }

    [HttpGet("user/{UserId}/content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetSavedContents(Guid UserId, Guid ContentId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpGet("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetCurrentUserSavedContents(Guid ContentId)
    {
        return NotFound("Dont released this end point");
    }


    [HttpDelete("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReSaveContent(Guid ContentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        Content? content = await context.Contents
            .FirstOrDefaultAsync(content => content.Id == ContentId);

        SavedContent? savedContent = await context.SavedContents
            .FirstOrDefaultAsync(savedContent =>
                savedContent.UserId == currentUserId &&
                savedContent.ContentId == ContentId);

        if (currentUser is null)
            return NotFound("User not found");
        if (content is null)
            return NotFound("Content not found");
        if (savedContent is null)
            return NotFound("Save dont placed");

        context.SavedContents.Remove(savedContent);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} re saved content {ContentId}",
          currentUserId, ContentId);

        return NoContent();
    }
}