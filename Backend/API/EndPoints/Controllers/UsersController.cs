using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Backend.API.Services;
using Backend.API.Dtos;
using Npgsql;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly IAuthService authService;
    public UsersController(EndlessContext context, IAuthService authService)
    {
        this.context = context;
        this.authService = authService;
    }

    [HttpPost]
    [EnableRateLimiting("RegistryLimit")]
    public async Task<IActionResult> CreateUser(AuthRequestDto requestDto)
    {
        AuthResponseDto? responseDto = await authService.RegistryAsync(requestDto);

        if (responseDto is null)
            return BadRequest("");

        this.CraeteTokensInCookies(responseDto);

        return Ok(responseDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        return Ok();
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetUsersForName(UserSearchRequestDto requestDto) //Searching
    {
        if (string.IsNullOrEmpty(requestDto.Name))
            return BadRequest("The name is empty");

        IQueryable<User> query = context.Users.AsQueryable();

        if (requestDto.LastSimilarity is not null)
        {
            query = query.Where(
                user => EF.Functions.TrigramsSimilarity(user.Name, requestDto.Name) < requestDto.LastSimilarity);
        }
        else
        {
            query = query.Where(user =>
                EF.Functions.ILike(user.Name, $"%{requestDto.Name}%") ||
                EF.Functions.TrigramsSimilarity(user.Name, requestDto.Name) > 0.2f ||
                EF.Functions.FuzzyStringMatchLevenshtein(user.Name, requestDto.Name) <= 3);
        }

        List<UserSearchResponseDto> users = await query
            .OrderByDescending(user => EF.Functions.TrigramsSimilarity(user.Name, requestDto.Name))
            .Take(25)
            .Select(
                user => new UserSearchResponseDto(
                    user.Id,
                    user.Name,
                    "@" + user.Slug,
                    user.Description ?? "",
                    user.RegistryData,
                    user.Email, user.Role,
                    user.TotalLikes,
                    user.CommentsCount,
                    user.ContentsCount,
                    user.FollowersCount,
                    user.FollowingCount,
                    user.OwnedDomainsCount,
                    user.DomainSubscriptionsCount,
                    EF.Functions.TrigramsSimilarity(user.Name, requestDto.Name)))
                .AsNoTracking().ToListAsync();

        return Ok(users);
    }

    //Current User
    [Authorize]
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user is null)
            return NotFound();

        return Ok(new UserResponseDto(
                    user.Id,
                    user.Name,
                    "@" + user.Slug,
                    user.Description ?? "",
                    user.RegistryData,
                    user.Email, user.Role,
                    user.TotalLikes,
                    user.CommentsCount,
                    user.ContentsCount,
                    user.FollowersCount,
                    user.FollowingCount,
                    user.OwnedDomainsCount,
                    user.DomainSubscriptionsCount));
    }

    [Authorize]
    [HttpPut("current")]
    public async Task<IActionResult> UpdateCurrentUser(UserUpdateDto updateDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user is null)
            return BadRequest();

        if (!string.IsNullOrEmpty(updateDto.Name))
        {
            user.Name = updateDto.Name;
            user.Slug = updateDto.Name.GenerateSlug();
        }

        if (!string.IsNullOrEmpty(updateDto.Description))
            user.Description = updateDto.Description;

        //for test
        user.Role = updateDto.Role;

        try
        {
            await context.SaveChangesAsync();

            return Ok(new UserResponseDto(
                    user.Id,
                    user.Name,
                    "@" + user.Slug,
                    user.Description ?? "",
                    user.RegistryData,
                    user.Email, user.Role,
                    user.TotalLikes,
                    user.CommentsCount,
                    user.ContentsCount,
                    user.FollowersCount,
                    user.FollowingCount,
                    user.OwnedDomainsCount,
                    user.DomainSubscriptionsCount));
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                return BadRequest($"This name {updateDto.Name} already existn");

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

        return NoContent();
    }
}