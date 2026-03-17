using Microsoft.AspNetCore.Authorization;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Extensions;
using Backend.API.Services;
using Backend.API.Dtos;
using Microsoft.AspNetCore.RateLimiting;

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
    [EnableRateLimiting("LoginLimit")]
    public async Task<ActionResult<AuthResponseDto>> Login(AuthRequestDto requestDto)
    {
        AuthResponseDto? responseDto = await authService.LoginAsync(requestDto);

        if (responseDto is null)
            return BadRequest("Password or Email dont correct");

        this.CraeteTokensInCookies(responseDto);

        return Ok(responseDto);
    }

    [HttpPut("token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken()
    {
        RefreshTokenRequestDto refreshDto = new(Request.Cookies["RefreshToken"]!);

        AuthResponseDto? responseDto = await authService.RefreshTokensAsync(refreshDto);

        if (responseDto is null || responseDto.Token is null || responseDto.RefreshToken is null)
            return BadRequest("Invalid refresh token");

        this.CraeteTokensInCookies(responseDto);

        return Ok(responseDto);
    }

    [Authorize()]
    [HttpDelete("token")]
    public IActionResult Logout()
    {
        this.DeleteTokensInCookies();

        return NoContent();
    }
}