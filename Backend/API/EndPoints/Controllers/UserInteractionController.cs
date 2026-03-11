
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

    public UserInteractionController(EndlessContext context, IInteractionService interaction)
    {
        this.context = context;

        this.interaction = interaction;
    }

    [HttpPut("{Id}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> CreateInteractionForContent(Guid Id)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        Content? content = await context.Contents.FindAsync(Id);

        if (currentUser is null || content is null)
            return BadRequest("User or Content not found");

        currentUser.UserInterationsCount++;

        UserInterationContent userInteration = new();
        userInteration.UserId = currentUserId;
        userInteration.ContentId = content.Id;

        userInteration.Liked = await context.LikedContents
            .AsNoTracking()
            .AnyAsync(l => l.OwnerId == currentUserId && l.ContentId == content.Id);

        userInteration.Saved = await context.SavedContents
            .AsNoTracking()
            .AnyAsync(s => s.OwnerId == currentUserId && s.ContentId == content.Id);


        return Ok();
    }


}