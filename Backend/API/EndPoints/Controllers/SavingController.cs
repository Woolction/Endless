using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Backend.API.Dtos;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SavingController : ControllerBase
{
    private readonly EndlessContext context;

    public SavingController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<ContentResponseDto>> SaveContent(Guid ContentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        var content = await context.Contents
            .Select(content => new
            {
                c = content,
                cResponse = new ContentResponseDto(
                    content.Id, content.DomainId, content.CreatorId,
                    content.Title, content.Slug, content.Description,
                    content.CreatedDate, content.ContentType.ToString(),
                    content.VideoMeta != null ? content.VideoMeta.DurationSeconds : 0,
                    content.ContentUrl, content.PrewievPhotoUrl, content.Savers.Count,
                    content.Likers.Count, content.Comments.Count, content.DizLikers.Count,
                    content.ViewsCount)
            })
            .FirstOrDefaultAsync(content => content.c.Id == ContentId);

        if (currentUser is null)
            return NotFound("User not found");
        if (content is null || content.c is null)
            return NotFound("Content not found");

        SavedContent savedContent = new()
        {
            UserId = currentUserId,
            ContentId = ContentId,
            SavedDate = DateTime.UtcNow
        };

        context.SavedContents.Add(savedContent);

        await context.SaveChangesAsync();

        return Created($"api/saving/user/{savedContent.UserId}/content/{savedContent.ContentId}",
            content.cResponse);
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

        return NoContent();
    }
}