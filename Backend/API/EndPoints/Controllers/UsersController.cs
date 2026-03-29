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
    private readonly IR2Service r2Service;
    public UsersController(EndlessContext context, IAuthService authService, IR2Service r2Service)
    {
        this.context = context;

        this.authService = authService;
        this.r2Service = r2Service;
    }

    [HttpPost]
    [EnableRateLimiting("RegistryLimit")]
    public async Task<ActionResult<AuthResponseDto>> CreateUser(AuthRequestDto requestDto)
    {
        AuthResponseDto? responseDto = await authService.RegistryAsync(requestDto);

        if (responseDto is null)
            return BadRequest("");

        this.CraeteTokensInCookies(responseDto);

        return Ok(responseDto);
    }

    [HttpGet("search")]
    public async Task<ActionResult<UserSearchResponseDto>> GetUsersForName([FromQuery] SearchRequestDto requestDto) //Searching
    {
        if (string.IsNullOrEmpty(requestDto.Name))
            return BadRequest("The name is empty");

        IQueryable<User> query = context.Users.AsQueryable();

        if (requestDto.LastSearch is not null)
        {
            query = query.Where(user =>
                EF.Functions.ILike(user.Name, $"%{requestDto.Name}%") == requestDto.LastSearch.LastLiked &&
                EF.Functions.TrigramsSimilarity(user.Name, requestDto.Name) < requestDto.LastSearch.LastSimilarity &&
                EF.Functions.FuzzyStringMatchLevenshtein(user.Name, requestDto.Name) >= requestDto.LastSearch.LastLevenshit);
        }
        else
        {
            query = query.Where(user =>
                EF.Functions.ILike(user.Name, $"%{requestDto.Name}%") ||
                EF.Functions.TrigramsSimilarity(user.Name, requestDto.Name) > 0.2f ||
                EF.Functions.FuzzyStringMatchLevenshtein(user.Name, requestDto.Name) <= 3);
        }

        var users = await query
            .OrderByDescending(user => EF.Functions.TrigramsSimilarity(user.Name, requestDto.Name))
            .Take(20).Select(user => new
            {
                User = new UserResponseDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                    user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedDomains.Count, user.SubscripedDomains.Count),
                LastLiked = EF.Functions.ILike(user.Name, $"%{requestDto.Name}%"),
                LastSimilarity = EF.Functions.TrigramsSimilarity(user.Name, requestDto.Name),
                LastLevenshit = EF.Functions.FuzzyStringMatchLevenshtein(user.Name, requestDto.Name)
            })
            .AsNoTracking().ToArrayAsync();

        var lastResponse = users.LastOrDefault();

        UserResponseDto[] userResponses = users.Select(user => user.User).ToArray();

        return Ok(new UserSearchResponseDto(
            userResponses, lastResponse == null ? null : GetSearchDto(
                lastResponse.LastLiked, lastResponse.LastSimilarity, lastResponse.LastLevenshit)));
    }

    [HttpGet("{UserId}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(Guid UserId)
    {
        UserResponseDto? userResponse = await context.Users
            .Select(user =>
                new UserResponseDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                    user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedDomains.Count, user.SubscripedDomains.Count))
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == UserId);

        if (userResponse == null)
            return BadRequest("User not found");

        return Ok(userResponse);
    }

    //Current User
    [Authorize]
    [HttpGet("current")]
    public async Task<ActionResult<UserResponseDto>> GetCurrentUser()
    {
        Guid currentUserId = this.GetIDFromClaim();

        UserResponseDto? user = await context.Users
            //.Where(u => u.Id == currentUserId)
            .Select(user => new UserResponseDto(
                user.Id, user.Name, "@" + user.Slug,
                user.Description ?? "", user.RegistryData, user.Email,
                user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                user.Comments.Count, user.Contents.Count, user.Followers.Count,
                user.Following.Count, user.OwnedDomains.Count, user.SubscripedDomains.Count))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [Authorize]
    [HttpPut("current")]
    public async Task<ActionResult<UserResponseDto>> UpdateCurrentUser(UserUpdateDto updateDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        var user = await context.Users
            .Select(user => new {
                u = user, uResponse = new UserResponseDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                    user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedDomains.Count, user.SubscripedDomains.Count)})
            .FirstOrDefaultAsync(user => user.u.Id == currentUserId);

        if (user is null || user.u is null)
            return BadRequest("User not found");

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

            return Ok(user.uResponse);
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

    private SearchDto GetSearchDto(bool IsLastLiked, double LastSimilarity, int LastLevenshit)
    {
        return new()
        {
            LastLiked = IsLastLiked,
            LastSimilarity = LastSimilarity,
            LastLevenshit = LastLevenshit
        };
    }
}