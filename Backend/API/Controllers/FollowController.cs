using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Managers;
using Backend.API.Dtos;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]/user")]
public class FollowController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly ILogger<FollowController> logger;

    public FollowController(EndlessContext context, ILogger<FollowController> logger)
    {
        this.context = context;

        this.logger = logger;
    }

    [HttpPost("{UserId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<UserResponseDto>> Following(Guid UserId)
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
                uResponse = new UserResponseDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                    user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedDomains.Count, user.SubscripedDomains.Count)
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