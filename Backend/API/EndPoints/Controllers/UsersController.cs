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
    public async Task<IActionResult> CreateUser(AuthRequestDto requestDto)
    {
        AuthResponseDto? responseDto = await authService.RegistryAsync(requestDto);

        if (responseDto is null)
            return BadRequest("");

        this.CraeteTokensInCookies(responseDto);

        return Ok(responseDto);
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetUsersForName(UserSearchRequestDto requestDto) //Searching
    {
        if (string.IsNullOrEmpty(requestDto.Name))
            return BadRequest("The name is empty");

        IQueryable<User> query = context.Users.AsQueryable();

        if (requestDto.LastSearch is not null)
        {
            query= query.Where(
                user => EF.Functions.TrigramsSimilarity(user.Name, requestDto.Name) < requestDto.LastSearch.LastSimilarity);
        }
        else
        {
            query = query
                .Where(user =>
                    EF.Functions.ILike(user.Name, $"%{requestDto.Name}%") ||
                    EF.Functions.TrigramsSimilarity(user.Name, requestDto.Name) > 0.2f ||
                    EF.Functions.FuzzyStringMatchLevenshtein(user.Name, requestDto.Name) <= 3);
        }

        UserResponseDto[] users = await query
            .OrderByDescending(user => EF.Functions.TrigramsSimilarity(user.Name, requestDto.Name))
            .Take(25).Select(user => user.GetUserResponseDto())
            .AsNoTracking().ToArrayAsync();

        UserResponseDto? lastResponse = users[^1];

        return Ok(new UserSearchResponseDto(users, GetSearchDto(lastResponse, requestDto)));
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

        return Ok(user.GetUserResponseDto());
    }

    [Authorize]
    [HttpPut("current")]
    public async Task<IActionResult> UpdateCurrentUser(UserUpdateDto updateDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user is null)
            return BadRequest("User not found");

        if (!string.IsNullOrEmpty(updateDto.Name))
        {
            user.Name = updateDto.Name;
            user.Slug = updateDto.Name.GenerateSlug();
        }

        if (!string.IsNullOrEmpty(updateDto.Description))
            user.Description = updateDto.Description;

        if (updateDto.AvatarPhoto is not null && updateDto.AvatarPhoto.Length != 0)
        {
            string photoPath = await r2Service.SaveFormFileAsync(updateDto.AvatarPhoto, "Images", ".jpeg");

            string photoUrl = await r2Service.SaveImage(photoPath);

            user.AvatarPhotoUrl = photoUrl;

            System.IO.File.Delete(photoPath);
        }

        //for test
        user.Role = updateDto.Role;

        try
        {
            await context.SaveChangesAsync();

            return Ok(user.GetUserResponseDto());
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

    private SearchDto? GetSearchDto(UserResponseDto? user, UserSearchRequestDto requestDto)
    {
        if (user is null)
            return null;

        return new()
        {
            LastLiked = EF.Functions.ILike(user.Name, $"%{requestDto.Name}%"),
            LastSimilarity = EF.Functions.TrigramsSimilarity(user.Name, requestDto.Name),
            LastLevenshit = EF.Functions.FuzzyStringMatchLevenshtein(user.Name, requestDto.Name)
        };
    }
}