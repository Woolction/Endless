using Microsoft.AspNetCore.Mvc;
using Backend.API.Services;
using Backend.API.Dtos;

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
        AuthResponseDto? token = await authService.LoginAsync(requestDto);

        if (token is null)
            return Unauthorized("Password or Email dont correct");

        return Ok(token);
    }

    [HttpPut("token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto requestDto)
    {
        AuthResponseDto? responseDto = await authService.RefreshTokensAsync(requestDto);

        if (responseDto is null || responseDto.Token is null || responseDto.RefreshToken is null)
            return Unauthorized("Invalid refresh token");

        return Ok(responseDto);
    }
}