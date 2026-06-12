using Microsoft.AspNetCore.Authorization;
using Application.Features.Rows.Contents;
using Application.Features.Users.Dtos;
using Microsoft.EntityFrameworkCore;
using Application.Features.Imagess;
using Application.Interfaces.Db;
using Application.Features.Dtos;
using Microsoft.AspNetCore.Mvc;
using Application.Utilities;
using Domain.Common.Enums;
using Domain.Entities;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]/user")]
public class FollowController : ControllerBase
{
    private readonly IAppDbContext context;

    private readonly ILogger<FollowController> logger;

    public FollowController(IAppDbContext context, ILogger<FollowController> logger)
    {
        this.context = context;

        this.logger = logger;
    }

    [HttpPost("{UserId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<UserDto>> Following(Guid UserId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        if (currentUserId == UserId)
        {
            logger.LogWarning("User {UserId} tried to following on yourself",
                currentUserId);
            return Forbid("You dont have a follow you");
        }

        User? currentUser = await context.Users.FindAsync(currentUserId);
        var user = await context.Users
            .Select(user => new
            {
                u = user,
                uResponse = new UserDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), new PhotoDto(
                        new ImageVariants(
                            user.UserMeta.IconBase,
                            user.UserMeta.Small,
                            user.UserMeta.Medium,
                            user.UserMeta.Large),
                        user.UserMeta.R,
                        user.UserMeta.G,
                        user.UserMeta.B),
                    user.TotalLikes, user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.u.Id == UserId);

        if (currentUser is null)
            return NotFound("Current user not found");
        if (user is null || user.u is null)
            return NotFound("User not found");

        UserFollowing userFollowing = new()
        {
            FollowerId = currentUserId,
            FollowedUserId = UserId,
            FollowedDate = DateTime.UtcNow
        };

        context.UserFollowings.Add(userFollowing);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} following to User {FollowedUserId}",
            currentUserId, UserId);

        return Created($"api/follow/follower/{userFollowing.FollowerId}/followed/{userFollowing.FollowedUserId}",
            user.uResponse);
    }

    [HttpGet("follower/{FollowerId}/followed/{FollowedId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetFollowings(Guid FollowerId, Guid FollowedId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpGet("follower/{FollowerId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetUserFollowings(Guid FollowerId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpGet("current")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetCurrentUserFollowings()
    {
        return NotFound("Dont released this end point");
    }


    [HttpDelete("{UserId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReFollowing(Guid UserId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        if (currentUserId == UserId)
        {
            logger.LogWarning("User {UserId} tried to re following on yourself",
               currentUserId);
            return Forbid("You dont have a refollow you");
        }

        UserFollowing? userFollowing = await context.UserFollowings
            .FirstOrDefaultAsync(userFollowing =>
                userFollowing.FollowerId == currentUserId &&
                userFollowing.FollowedUserId == UserId);

        if (userFollowing is null)
            return NotFound("Followed User not found");

        context.UserFollowings.Remove(userFollowing);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} re following to User {FollowedUserId}",
           currentUserId, UserId);

        return NoContent();
    }
}