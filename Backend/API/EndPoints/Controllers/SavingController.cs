using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]/content")]
public class SavingController : ControllerBase
{
    private readonly EndlessContext context;

    public SavingController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpPost("{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> SaveContent(Guid ContentId)
    {
        return Ok();
    }

    [HttpDelete("{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReSaveContent(Guid ContentId)
    {
        return NoContent();
    }
}