using Microsoft.AspNetCore.Authorization;
using Backend.API.Data.Components;
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
    public async Task<IActionResult> Login(AuthRequestDto requestDto)
    {
        AuthResponseDto? responseDto = await authService.LoginAsync(requestDto);

        if (responseDto is null)
            return Unauthorized("Password or Email dont correct");

        CraeteTokensInCookies(responseDto);

        return Ok(responseDto);
    }

    [HttpPut("token")]
    public async Task<IActionResult> RefreshToken()
    {
        RefreshTokenRequestDto refreshDto = new(Request.Cookies["RefreshToken"]!);

        AuthResponseDto? responseDto = await authService.RefreshTokensAsync(refreshDto);

        if (responseDto is null || responseDto.Token is null || responseDto.RefreshToken is null)
            return Unauthorized("Invalid refresh token");

        CraeteTokensInCookies(responseDto);

        return Ok(responseDto);
    }

    [HttpGet("admin")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public IActionResult TestAdmin()
    {
        return Ok("Hi Admin");
    }

    private void CraeteTokensInCookies(AuthResponseDto responseDto)
    {
        Response.Cookies.Append("AccessToken", responseDto.Token, new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });

        Response.Cookies.Append("RefreshToken", responseDto.RefreshToken, new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(14)
        });
    }
}