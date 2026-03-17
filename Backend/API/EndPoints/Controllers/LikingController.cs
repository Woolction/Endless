using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]/content")]
public class LikingController : ControllerBase
{
    private readonly EndlessContext context;

    public LikingController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpPost("{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> LikeContent(Guid ContentId)
    {
        return Ok();
    }

    [HttpDelete("{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReLikeContent(Guid ContentId)
    {
        return NoContent();
    }

    //DizLikes in other class
}