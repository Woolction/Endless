using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Following(Guid UserId)
    {
        return Ok();
    }

    [HttpDelete("{UserId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReFollowing(Guid UserId)
    {
        return NoContent();
    }
}