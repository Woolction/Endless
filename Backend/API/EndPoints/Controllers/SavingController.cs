using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Dtos;
using Backend.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        Content? content = await context.Contents
            .Include(content => content.Creator)
            .FirstOrDefaultAsync(content => content.Id == ContentId);

        if (currentUser is null)
            return BadRequest("User not found");
        if (content is null)
            return BadRequest("Content not found");

        currentUser.SavedContentsCount++;
        
        content.SavesCount++;
        content.Creator!.ContentsSavesCount++;

        SavedContent savedContent = new()
        {
            UserId = currentUserId,
            ContentId = ContentId,
            SavedDate = DateTime.UtcNow
        };

        context.SavedContents.Add(savedContent);

        await context.SaveChangesAsync();

        return Ok(content.GetContentResponseDto());
    }

    [HttpDelete("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReSaveContent(Guid ContentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        Content? content = await context.Contents
            .Include(content => content.Creator)
            .FirstOrDefaultAsync(content => content.Id == ContentId);

        SavedContent? savedContent = await context.SavedContents
            .FirstOrDefaultAsync(savedContent =>
                savedContent.UserId == currentUserId &&
                savedContent.ContentId == ContentId);

        if (currentUser is null)
            return BadRequest("User not found");
        if (content is null)
            return BadRequest("Content not found");
        if (savedContent is null)
            return BadRequest("Save dont placed");

        currentUser.SavedContentsCount--;

        content.SavesCount--;
        content.Creator!.ContentsSavesCount--;

        context.SavedContents.Remove(savedContent);

        await context.SaveChangesAsync();

        return NoContent();
    }
}