using Microsoft.AspNetCore.Authorization;
using Application.Features.Users.Dtos;
using Microsoft.EntityFrameworkCore;
using Application.Features.Images;
using Application.Interfaces.Db;
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

        if (currentUser is null)
            return NotFound("Current user not found");

        var userDto = await context.Users
            .Where(user => user.Id == UserId)
            .Select(user => new UserDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData,
                    user.Email, user.Role.ToString(),
                    new ImageDto(
                        new ImageVariantsDto(
                            user.Meta.Image.BaseUrl,
                            user.Meta.Image.Variants
                                .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                                .ToList()),
                        user.Meta.Image.R,
                        user.Meta.Image.G,
                        user.Meta.Image.B),
                    user.TotalLikes, user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (userDto is null)
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
            userDto);
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