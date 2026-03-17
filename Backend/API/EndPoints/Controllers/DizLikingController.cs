using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DizLikingController : ControllerBase
{
    private readonly EndlessContext context;

    public DizLikingController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> DizLikeContent(Guid ContentId)
    {
        return Ok();
    }

    [HttpDelete("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReDizLikeContent(Guid ContentId)
    {
        return NoContent();
    }

    [HttpPost("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> DizLikeComment(Guid CommentId)
    {
        return Ok();
    }

    [HttpDelete("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReDizLikeComment(Guid CommentId)
    {
        return NoContent();
    }
}