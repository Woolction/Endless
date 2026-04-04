using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Backend.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Managers;
using Backend.API.Dtos;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;

    private readonly ILogger<AuthController> logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        this.authService = authService;

        this.logger = logger;
    }

    [HttpGet("token")]
    [EnableRateLimiting("LoginLimit")]
    public async Task<ActionResult<AuthResponseDto>> Login(AuthRequestDto requestDto)
    {
        AuthResponseDto? responseDto = await authService.LoginAsync(requestDto);

        if (responseDto is null)
            return BadRequest("Password or Email dont correct");

        logger.LogInformation("User {UserId} Logined", this.GetIDFromClaim());

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

        logger.LogInformation("User {UserId} Refreshed Token", this.GetIDFromClaim());

        this.CraeteTokensInCookies(responseDto);

        return Ok(responseDto);
    }

    [Authorize()]
    [HttpDelete("token")]
    public IActionResult Logout()
    {
        logger.LogInformation("User {UserId} Logout", this.GetIDFromClaim());

        this.DeleteTokensInCookies();

        return NoContent();
    }
}