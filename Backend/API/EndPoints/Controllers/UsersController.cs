using Microsoft.AspNetCore.Identity; 
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Model;
using Backend.API.Dtos;
using Backend.API.Services;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IAuthService authService;
    public UsersController(IAuthService authService)
    {
        this.authService = authService;
    }
    
    [HttpPost] 
    public async Task<IActionResult> UserRegistry([FromBody] AuthRequestDto requestDto)
    {
        User? user = await authService.RegistryAsync(requestDto);

        if (user == null)
            return BadRequest("");

        return Ok("User Registred");
    }
}