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
[Route("api/[controller]/user")]
public class FollowController : ControllerBase
{
    private readonly EndlessContext context;

    public FollowController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpPost("{UserId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<UserResponseDto>> Following(Guid UserId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        if (currentUserId == UserId)
            return BadRequest("You dont have a follow you");

        User? currentUser = await context.Users.FindAsync(currentUserId);
        User? user = await context.Users.FindAsync(UserId);

        if (currentUser is null)
            return BadRequest("Current user not found");
        if (user is null)
            return BadRequest("User not found");

        UserFollowing userFollowing = new()
        {
            FollowerId = currentUserId,
            FollowedUserId = UserId,
            FollowedDate = DateTime.UtcNow
        };

        context.UserFollowings.Add(userFollowing);

        await context.SaveChangesAsync();

        return Ok(user.GetUserResponseDto());
    }

    [HttpDelete("{UserId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReFollowing(Guid UserId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        if (currentUserId == UserId)
            return BadRequest("You dont have a refollow you");

        UserFollowing? userFollowing = await context.UserFollowings
            .FirstOrDefaultAsync(userFollowing =>
                userFollowing.FollowerId == currentUserId &&
                userFollowing.FollowedUserId == UserId);

        if (userFollowing is null)
            return BadRequest("Followed User not found");

        context.UserFollowings.Remove(userFollowing);

        await context.SaveChangesAsync();

        return NoContent();
    }
}