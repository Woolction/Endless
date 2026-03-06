using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Model;
using Backend.API.Services;
using Backend.API.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;

    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    [HttpPost("token")]
    public async Task<IActionResult> Login([FromBody] AuthRequestDto requestDto)
    {
        string? token = await authService.LoginAsync(requestDto);

        if (token is null)
            return Unauthorized("Password or Email dont correct");

        return Ok(new AuthResponseDto(token));
    }

    [HttpGet("users")]
    [Authorize]
    public IActionResult OnlyUsers()
    {
        return Ok("Hi user");
    }

    [HttpGet("admin")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public IActionResult OnlyAdmin()
    {
        return Ok("Hi Admin");
    }
}