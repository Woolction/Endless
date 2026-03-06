using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Models;
using Backend.API.Services;
using Backend.API.Dtos;

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

        UserDto userDto = new(user.Id, user.Email);

        return Ok($"User Registred: {userDto}");
    }
}