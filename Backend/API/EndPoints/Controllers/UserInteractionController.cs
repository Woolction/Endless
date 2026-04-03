using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Backend.API.Services;
using Backend.API.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

[ApiController]
[Route("api/[user][controller]")]
public class UserInteractionController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly IInteractionService interaction;

    public UserInteractionController(EndlessContext context, IInteractionService interaction)
    {
        this.context = context;

        this.interaction = interaction;
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<GenreVectorsResponse>> CreateInteractionForContent(Guid ContentId, int watchTimeSeconds)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        Content? content = await context.Contents
            .Include(content => content.VideoMeta)
            .FirstOrDefaultAsync(content => content.Id == ContentId);

        if (currentUser == null || content == null || content.VideoMeta == null)
            return NotFound("User or Content or Meta not found");

        UserInterationContent? userInteraction = await context.UserInterationContents.FindAsync(currentUserId);

        if (userInteraction is null)
        {
            userInteraction = new()
            {
                UserId = currentUserId,
                ContentId = content.Id,
            };

            context.UserInterationContents.Add(userInteraction);
        }

        userInteraction.Liked = await context.LikedContents
            .AsNoTracking()
            .AnyAsync(l => l.UserId == currentUserId && l.ContentId == content.Id);

        userInteraction.Saved = await context.SavedContents
            .AsNoTracking()
            .AnyAsync(s => s.UserId == currentUserId && s.ContentId == content.Id);

        userInteraction.WatchTimeSeconds = watchTimeSeconds;

        await context.SaveChangesAsync();

        UserGenreVector[] userGenres = await context.UserVectors
            .Include(uG => uG.Genre)
            .OrderBy(uG => uG.Genre!.Order)
            .Where(uG => uG.UserId == currentUserId)
            .ToArrayAsync();

        ContentGenreVector[] contentGenres = await context.ContentVectors
            .Include(cG => cG.Genre)
            .OrderBy(cG => cG.Genre!.Order)
            .Where(cG => cG.ContentId == content.Id)
            .ToArrayAsync();

        GenreInfo genreInfo = await context.GenreInfos.AsNoTracking().FirstAsync();

        interaction.Interaction(userGenres, content, contentGenres, userInteraction, genreInfo.Count);

        return Created($"api/interaction/user/{userInteraction.UserId}/content/{userInteraction.ContentId}",
            new GenreVectorsResponse(
                userGenres.GetUserGenreVectors(),
                contentGenres.GetContentGenreVectors()));
    }

    [HttpGet("user/{UserId}/content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetInteraction(Guid UserId, Guid ContentId)
    {
        return NotFound("Dont released this end point");
    }
}