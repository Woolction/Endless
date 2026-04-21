using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Application.Users.Create.Registry;
using Application.Users.Create.Many;
using Application.Users.Search;
using Application.Users.Update;
using Application.Users.Choose;
using Application.Users.Delete;
using Microsoft.AspNetCore.Mvc;
using Application.Users.Dtos;
using Application.Utilities;
using Domain.Common;
using Application;
using MediatR;
using Application.Authentications.Dtos;
using Application.Authentications.Update;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator mediator;

    public UsersController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost]
    //[EnableRateLimiting("RegistryLimit")]
    public async Task<ActionResult<RegistryDto>> CreateUser(UserRegistryCommand cmd)
    {
        Result<RegistryDto> result = await mediator.Send(cmd);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                409 => Conflict(result.Error),
                500 => StatusCode(result.StatusCode, result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        this.CraeteTokensInCookies(result.Data!.Token, result.Data.RefreshToken);

        return Created($"api/users/{result.Data.NewUserId}", result.Data);
    }

    [HttpPost("many")]
    [Authorize(Policy = nameof(UserRole.Admin))]
    public async Task<ActionResult<UserDto[]>> CreateUsers(UsersCreateCommand cmd)
    {
        if (cmd.Names.Length < 1)
            return BadRequest($"you must write at least one name: {cmd.Names.Length}");

        Result<UserDto[]> result = await mediator.Send(cmd);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                409 => Conflict(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Created("", result.Data);
    }

    [HttpGet("search")]
    public async Task<ActionResult<SearchedUserDto[]>> SearchUsersByName([FromQuery] UserSearchQuery query)
    {
        if (string.IsNullOrEmpty(query.Name))
            return BadRequest("The name is empty");

        Result<SearchedUserDto[]> result = await mediator.Send(query);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Ok(result.Data);
    }

    [HttpGet("Count/{UserId}")]
    public async Task<ActionResult<UserDto>> GetUserCounts(Guid UserId)
    {
        return null;
    }

    [HttpGet("{UserId}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid UserId)
    {
        UserChooseQuery userQuery = new(UserId);

        Result<UserDto> resultUser = await mediator.Send(userQuery);

        if (!resultUser.IsSuccess || resultUser.Data == null)
        {
            return resultUser.StatusCode switch
            {
                404 => NotFound(resultUser.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Ok(resultUser.Data);
    }

    //Current User
    [Authorize]
    [HttpGet("current")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        Guid currentUserId = this.GetIDFromClaim();

        UserChooseQuery userQuery = new(currentUserId);

        Result<UserDto> resultUser = await mediator.Send(userQuery);

        if (!resultUser.IsSuccess || resultUser.Data == null)
        {
            return resultUser.StatusCode switch
            {
                404 => NotFound(resultUser.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Ok(resultUser.Data);
    }

    [Authorize]
    [HttpPut("current")]
    public async Task<ActionResult<UserDto>> UpdateCurrentUser(UserUpdateRequest request)
    {
        UserUpdateCommand cmd = new(
            this.GetIDFromClaim(), request.Name,
            request.Description, request.Role,
            request.AvatarPhoto);

        Result<UserDto> result = await mediator.Send(cmd);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                409 => Conflict(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        string? refreshToken = Request.Cookies["RefreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest("There is no refresh token");

        Result<AuthDto> authResult = await mediator.Send(new RefreshTokenCommand(refreshToken));

        if (!authResult.IsSuccess || authResult.Data == null)
        {
            if (authResult.Data is null || authResult.Data.Token is null || authResult.Data.RefreshToken is null)
                return BadRequest("Invalid refresh token");

            return authResult.StatusCode switch
            {
                400 => BadRequest(authResult.Error),
                404 => NotFound(authResult.Error),
                500 => StatusCode(authResult.StatusCode, authResult.Error),
                _ => StatusCode(500, "Unknown error")
            };
        }

        this.CraeteTokensInCookies(authResult.Data.Token, authResult.Data.RefreshToken);

        return Ok(result.Data);
    }

    [Authorize]
    [HttpDelete("current")]
    public async Task<IActionResult> DeleteCurrentUser()
    {
        Result<Null> result = await mediator.Send(new UserDeleteCommand(
            this.GetIDFromClaim()));

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.StatusCode),
                _ => StatusCode(500, "unknown error")
            };
        }

        this.DeleteTokensInCookies();

        return NoContent();
    }
}