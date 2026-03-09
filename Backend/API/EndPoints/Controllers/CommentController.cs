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

    [HttpGet("{ContentId}")]
    public async Task<IActionResult> GetComments(Guid ContentId)
    {
        return Ok();
    }

    [HttpPost("{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> CreateComment(Guid ContentId)
    {
        return Ok();
    }

    [HttpPut("{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> UpdateComment(Guid CommentId)
    {
        return Ok();
    }

    [HttpDelete("{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> DeleteComment(Guid CommentId)
    {
        return NoContent();
    }
}