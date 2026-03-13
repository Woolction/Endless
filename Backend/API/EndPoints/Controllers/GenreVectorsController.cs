using Microsoft.AspNetCore.Authorization;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenreVectrosController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Admin))]
    public async Task<IActionResult> CreateGenreVector()
    {
        return Ok();
    }
}