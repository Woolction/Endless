using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.EndPoints.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContentController : ControllerBase
{
    private readonly EndlessContext context;

    public ContentController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetContentForRecommendation()
    {
        return Ok();
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetContentForName()
    {
        return Ok();
    }

    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> CreateContent()
    {
        return Ok();
    }

    [HttpPut]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> UpdateContent()
    {
        return Ok();
    }

    [HttpDelete]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> DeleteContent()
    {
        return NoContent();
    }
}