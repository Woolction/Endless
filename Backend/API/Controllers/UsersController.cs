using Application.Authentications.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Application.Searchs;
using Application.Users.Create;
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
using Application.Users.Registry;
using MediatR;

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
            if (result.StatusCode == 409 && result.ResultId == 1)
                return Conflict(result.Error);

            return result.StatusCode switch
            {
                409 => Conflict(result.Error),
                500 => StatusCode(result.StatusCode, result.Error),
                _ => StatusCode(500, "Unknown error")
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

        if (!result.IsSuccess)
        {
            return result.StatusCode switch
            {
                409 => Conflict(result.Error),
                _ => StatusCode(500, "Unknown error")
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
                _ => StatusCode(500, "Unknown error")
            };
        }

        logger.LogInformation("Search returned users: {Count} results for {Query}",
            result.Data.UserDtos.Length, query.Name);

        return Ok(result.Data);
    }

    [HttpGet("{UserId}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid UserId)
    {
        UserDto? userResponse = await context.Users
            .Where(user => user.Id == UserId)
            .Select(user =>
                new UserDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                    user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (userResponse == null)
            return NotFound();

        logger.LogInformation("Returned user {UserId}",
            UserId);

        return Ok(userResponse);
    }

    //Current User
    [Authorize]
    [HttpGet("current")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        Guid currentUserId = this.GetIDFromClaim();

        UserDto? user = await context.Users
            .Where(user => user.Id == currentUserId)
            .Select(user => new UserDto(
                user.Id, user.Name, "@" + user.Slug,
                user.Description ?? "", user.RegistryData, user.Email,
                user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                user.Comments.Count, user.Contents.Count, user.Followers.Count,
                user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (user is null)
            return NotFound();

        logger.LogInformation("Returned User {UserId}", currentUserId);

        return Ok(user);
    }

    [Authorize]
    [HttpPut("current")]
    public async Task<ActionResult<UserDto>> UpdateCurrentUser(UserUpdateCommand updateDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        var user = await context.Users
            .Select(user => new
            {
                u = user,
                CommentsCount = user.Comments.Count,
                ContentsCount = user.Contents.Count,
                FollowersCount = user.Followers.Count,
                FollowingCount = user.Following.Count,
                OwnedChannelsCount = user.OwnedChannels.Count,
                SubscripedChannelsCount = user.SubscripedChannels.Count
            })
            .FirstOrDefaultAsync(user => user.u.Id == currentUserId);

        if (user is null || user.u is null)
            return NotFound();

        if (!string.IsNullOrEmpty(updateDto.Name))
        {
            user.u.Name = updateDto.Name;
            user.u.Slug = updateDto.Name.GenerateSlug();
        }

        if (!string.IsNullOrEmpty(updateDto.Description))
            user.u.Description = updateDto.Description;

        if (updateDto.AvatarPhoto is not null && updateDto.AvatarPhoto.Length != 0)
        {
            string photoPath = await r2Service.SaveFormFileAsync(updateDto.AvatarPhoto, "Images", ".jpeg");

            string photoUrl = await r2Service.SaveImage(photoPath);

            user.u.AvatarPhotoUrl = photoUrl;

            System.IO.File.Delete(photoPath);
        }

        //for test
        user.u.Role = updateDto.Role;

        try
        {
            await context.SaveChangesAsync();

            logger.LogInformation("User {UserId} updated", currentUserId);

            UserDto userDto = new(
                    user.u.Id,
                    user.u.Name,
                    "@" + user.u.Slug,
                    user.u.Description ?? "",
                    user.u.RegistryData,
                    user.u.Email,
                    user.u.Role.ToString(),
                    user.u.AvatarPhotoUrl,
                    user.u.TotalLikes,
                    user.CommentsCount,
                    user.ContentsCount,
                    user.FollowersCount,
                    user.FollowingCount,
                    user.OwnedChannelsCount,
                    user.SubscripedChannelsCount);

            return Ok(userDto);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error while updating user {UserId}", currentUserId);

            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                return Conflict($"This name {updateDto.Name} already existn");

            throw;
        }

    }

    [Authorize]
    [HttpDelete("current")]
    public async Task<IActionResult> DeleteCurrentUser()
    {
        Guid currentUserId = this.GetIDFromClaim();

        await context.Users.Where(user => user.Id == currentUserId).ExecuteDeleteAsync();

        this.DeleteTokensInCookies();

        logger.LogInformation("User {UserId} deleted", currentUserId);

        return NoContent();
    }
}