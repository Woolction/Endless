using Application.Authentications.Login;
using Application.Authentications.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Application.Authentications.Dtos;
using Microsoft.AspNetCore.Mvc;
using Application.Utilities;
using Application;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserUpdateTokenHandler updateTokenHandler;
    private readonly UserLoginHandler loginHandler;

    private readonly ILogger<AuthController> logger;

    public AuthController(UserLoginHandler loginHandler, UserUpdateTokenHandler updateTokenHandler, ILogger<AuthController> logger)
    {
        this.updateTokenHandler = updateTokenHandler;
        this.loginHandler = loginHandler;

        this.logger = logger;
    }

    [HttpGet("token")]
    [EnableRateLimiting("LoginLimit")]
    public async Task<ActionResult<AuthDto>> Login([FromQuery] AuthCreateCommand cmd)
    {
        Result<AuthDto> result = await loginHandler.Handle(cmd);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                400 => BadRequest(result.Error),
                404 => NotFound(result.Error),
                500 => StatusCode(result.StatusCode, result.Error),
                _ => StatusCode(500, "Unknown error")
            };
        }

        logger.LogInformation("User {UserId} Logined", result.Data!.UserId);

        this.CraeteTokensInCookies(result.Data.Token, result.Data.RefreshToken);

        return Ok(result.Data);
    }

    [HttpPut("token")]
    public async Task<ActionResult<AuthDto>> RefreshToken()
    {
        string? refreshToken = Request.Cookies["RefreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest("There is no refresh token");

        Result<AuthDto> result = await updateTokenHandler.Handle(new RefreshTokenCommand(refreshToken));

        if (!result.IsSuccess || result.Data == null)
        {
            if (result.Data is null || result.Data.Token is null || result.Data.RefreshToken is null)
                return BadRequest("Invalid refresh token");

            return result.StatusCode switch
            {
                400 => BadRequest(result.Error),
                404 => NotFound(result.Error),
                500 => StatusCode(result.StatusCode, result.Error),
                _ => StatusCode(500, "Unknown error")
            };
        }

        logger.LogInformation("User {UserId} Refreshed Token", result.Data!.UserId);

        this.CraeteTokensInCookies(result.Data.Token, result.Data.RefreshToken);

        return Ok(result.Data);
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