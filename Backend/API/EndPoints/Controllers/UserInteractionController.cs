
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Dtos;
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

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))] //<GenreVectorsResponse>
    public async Task<IActionResult> CreateInteractionForContent(Guid ContentId, int watchTimeSeconds)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        Content? content = await context.Contents
            .Include(content => content.VideoMeta)
            .FirstOrDefaultAsync(content => content.Id == ContentId);

        if (currentUser == null || content == null || content.VideoMeta == null)
            return BadRequest("User or Content or Meta not found");

        UserInterationContent? userInteration = await context.UserInterationContents.FindAsync(currentUserId);

        if (userInteration is null)
        {
            userInteration = new()
            {
                UserId = currentUserId,
                ContentId = content.Id,
            };

            context.UserInterationContents.Add(userInteration);
        }

        userInteration.Liked = await context.LikedContents
            .AsNoTracking()
            .AnyAsync(l => l.UserId == currentUserId && l.ContentId == content.Id);

        userInteration.Saved = await context.SavedContents
            .AsNoTracking()
            .AnyAsync(s => s.UserId == currentUserId && s.ContentId == content.Id);

        userInteration.WatchTimeSeconds = watchTimeSeconds;

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

        interaction.Interaction(userGenres, content, contentGenres, userInteration, genreInfo.Count);

        return Ok(new GenreVectorsResponse(
            userGenres.GetUserGenreVectors(),
            contentGenres.GetContentGenreVectors()));
    }
}