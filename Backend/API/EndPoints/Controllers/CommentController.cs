using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly EndlessContext context;

    public CommentController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> SendComment(Guid ContentId)
    {
        return Ok();
    }

    [HttpPut("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> UpdateComment(Guid CommentId)
    {
        return Ok();
    }

    [HttpDelete("comment/{CommentId}")]
    public async Task<IActionResult> DeleteComment(Guid CommentId)
    {
        return NoContent();
    }
}