
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Backend.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class UserInteractionController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly IInteractionService interaction;
    private readonly IFfmpegService ffmpegService;

    public UserInteractionController(EndlessContext context, IInteractionService interaction, IFfmpegService ffmpegService)
    {
        this.context = context;

        this.interaction = interaction;
        this.ffmpegService = ffmpegService;
    }

    [HttpPost("{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> CreateInteractionForContent(Guid ContentId, int watchTimeSeconds)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        Content? content = await context.Contents.FindAsync(ContentId);

        if (currentUser is null || content is null)
            return BadRequest("User or Content not found");

        UserInterationContent? userInteration = await context.UserInterationContents.FindAsync(currentUserId);

        if (userInteration is null)
        {
            currentUser.UserInterationsCount++;

            userInteration = new()
            {
                UserId = currentUserId,
                ContentId = content.Id,

                Liked = await context.LikedContents
                    .AsNoTracking()
                    .AnyAsync(l => l.OwnerId == currentUserId && l.ContentId == content.Id),

                Saved = await context.SavedContents
                    .AsNoTracking()
                    .AnyAsync(s => s.OwnerId == currentUserId && s.ContentId == content.Id)
            };
        }

        userInteration.WatchTimeSeconds = watchTimeSeconds;

        await context.SaveChangesAsync();

        interaction.Interaction(currentUser, content, userInteration);

        return Ok();
    }
}