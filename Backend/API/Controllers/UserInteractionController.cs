using Microsoft.AspNetCore.Authorization;
using Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Domain.Components;
using Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using API.Extensions;
using Application.Utilities;
using Contracts.Dtos.Genres;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserInteractionController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly ILogger<UserInteractionController> logger;
    private readonly IInteractionService interaction;

    public UserInteractionController(EndlessContext context, ILogger<UserInteractionController> logger, IInteractionService interaction)
    {
        this.context = context;

        this.interaction = interaction;
        this.logger = logger;
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<GenreVectorsDto>> CreateInteractionForContent(Guid ContentId, int watchTimeSeconds)
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

        logger.LogInformation("User {UserId} created an interaction on the content {ContentId}",
            currentUserId, ContentId);

        return Created($"api/interaction/user/{userInteraction.UserId}/content/{userInteraction.ContentId}",
            new GenreVectorsDto(
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