using Application.Commands.Authentications;
using Contracts.Dtos.Authentications;
using Application.Queries.Searchs;
using Application.Commands.Users;
using Contracts.Dtos.Searchs;
using Contracts.Dtos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Context;
using Domain.Entities;
using Application.Utilities;
using Npgsql;
using Application.Handlers;
using Application;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly UserRegistryHandler registryHandler;

    private readonly ILogger<UsersController> logger;
    private readonly IR2Service r2Service;

    public UsersController(EndlessContext context, UserRegistryHandler registryHandler, IAuthService authService, IR2Service r2Service, ILogger<UsersController> logger)
    {
        this.context = context;
        this.registryHandler = registryHandler;

        this.r2Service = r2Service;
        this.logger = logger;
    }

    [HttpPost]
    [EnableRateLimiting("RegistryLimit")]
    public async Task<ActionResult<RegistryDto>> CreateUser(AuthCreateCommand cmd)
    {
        Result<RegistryDto> result = await registryHandler.Handle(cmd);

        if (!result.IsSuccess)
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

    [HttpGet("search")]
    public async Task<ActionResult<UserSearchQuery>> GetUsersForName([FromQuery] SearchQuery searchQuery) //Searching
    {
        if (string.IsNullOrEmpty(searchQuery.Name))
            return BadRequest("The name is empty");

        IQueryable<User> query = context.Users.AsQueryable();

        if (searchQuery.LastSearch is not null)
        {
            query = query.Where(user =>
                EF.Functions.ILike(user.Name, $"%{searchQuery.Name}%") == searchQuery.LastSearch.LastLiked &&
                EF.Functions.TrigramsSimilarity(user.Name, searchQuery.Name) < searchQuery.LastSearch.LastSimilarity &&
                EF.Functions.FuzzyStringMatchLevenshtein(user.Name, searchQuery.Name) >= searchQuery.LastSearch.LastLevenshit);
        }
        else
        {
            query = query.Where(user =>
                EF.Functions.ILike(user.Name, $"%{searchQuery.Name}%") ||
                EF.Functions.TrigramsSimilarity(user.Name, searchQuery.Name) > 0.2f ||
                EF.Functions.FuzzyStringMatchLevenshtein(user.Name, searchQuery.Name) <= 3);
        }

        var users = await query
            .OrderByDescending(user => EF.Functions.TrigramsSimilarity(user.Name, searchQuery.Name))
            .Take(20).Select(user => new
            {
                User = new UserDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                    user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count),
                LastLiked = EF.Functions.ILike(user.Name, $"%{searchQuery.Name}%"),
                LastSimilarity = EF.Functions.TrigramsSimilarity(user.Name, searchQuery.Name),
                LastLevenshit = EF.Functions.FuzzyStringMatchLevenshtein(user.Name, searchQuery.Name)
            })
            .AsNoTracking().ToArrayAsync();

        var lastResponse = users.LastOrDefault();

        UserDto[] userResponses = users.Select(user => user.User).ToArray();

        logger.LogInformation("Search returned users: {Count} results for {Query}",
            userResponses.Length, searchQuery.Name);

        return Ok(new UserSearchQuery(
            userResponses, lastResponse == null ? null : GetSearchQuery(
                lastResponse.LastLiked, lastResponse.LastSimilarity, lastResponse.LastLevenshit)));
    }

    [HttpGet("{UserId}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid UserId)
    {
        UserDto? userResponse = await context.Users
            .Select(user =>
                new UserDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                    user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count))
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == UserId);

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
            .Select(user => new UserDto(
                user.Id, user.Name, "@" + user.Slug,
                user.Description ?? "", user.RegistryData, user.Email,
                user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                user.Comments.Count, user.Contents.Count, user.Followers.Count,
                user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count))
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

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
                uResponse = new UserDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                    user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count)
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

            return Ok(user.uResponse);
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

    private SearchDto GetSearchQuery(bool IsLastLiked, double LastSimilarity, int LastLevenshit)
    {
        return new()
        {
            LastLiked = IsLastLiked,
            LastSimilarity = LastSimilarity,
            LastLevenshit = LastLevenshit
        };
    }
}
