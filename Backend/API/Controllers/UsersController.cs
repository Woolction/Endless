using Application.Authentications.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Application.Searchs;
using Application.Users.Create.Many;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Context;
using Application.Utilities;
using Application.Channels;
using Application.Users.Dtos;
using Application;
using Npgsql;
using Domain.Common;
using Domain.Entities;
using Application.Users.Search;
using Application.Users.Update;
using Application.Users.Create.Registry;
using MediatR;
using Application.Users.Choose;
using Application.Users.Delete;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly EndlessContext context;
    private readonly IMediator mediator;

    private readonly ILogger<UsersController> logger;
    private readonly IR2Service r2Service;

    public UsersController(EndlessContext context, IMediator mediator, IR2Service r2Service, ILogger<UsersController> logger)
    {
        this.context = context;
        this.mediator = mediator;

        this.r2Service = r2Service;
        this.logger = logger;
    }

    [HttpPost]
    [EnableRateLimiting("RegistryLimit")]
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

        logger.LogInformation("User {UserId} registred",
            result.Data.NewUserId);

        return Created($"api/users/{result.Data.NewUserId}", result.Data);
    }

    [HttpPost("many")]
    [Authorize(Policy = nameof(UserRole.Admin))]
    public async Task<ActionResult<UserDto[]>> CreateUsers(UsersCreateCommand cmd)
    {
        if (cmd.Count < 1)
            return BadRequest($"Count < 1: {cmd.Count}");

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
    public async Task<ActionResult<UserSearchDto>> SearchUsersByName([FromQuery] UserSearchQuery query)
    {
        if (string.IsNullOrEmpty(query.Name))
            return BadRequest("The name is empty");

        Result<UserSearchDto> result = await mediator.Send(query);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        logger.LogInformation("Search returned users: {Count} results for {Query}",
            result.Data.UserDtos.Length, query.Name);

        return Ok(result.Data);
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

        return Ok(result.Data);
    }

    [Authorize]
    [HttpDelete("current")]
    public async Task<IActionResult> DeleteCurrentUser()
    {
        Guid currentUserId = this.GetIDFromClaim();

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