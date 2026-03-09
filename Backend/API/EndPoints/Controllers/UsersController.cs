using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using System.Security.Claims;
using Backend.API.Services;
using Backend.API.Dtos;
using Backend.API.Extensions;
using Microsoft.AspNetCore.RateLimiting;

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
    public async Task<IActionResult> GetUsersForSlug(string slug) //Searching
    {
        if (string.IsNullOrEmpty(slug))
            return BadRequest("The name is empty");

        slug.GenerateSlug();

        List<UserResponseDto> users = await context.Users.Where(
            user => user.Slug.Contains(slug)).Select(
                user => new UserResponseDto(
                    user.Id, user.Name, "@" + user.Slug, user.Description ?? "", user.RegistryData, user.Email, user.Role))
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
            user.Id, user.Name, "@" + user.Slug, user.Description ?? "", user.RegistryData, user.Email, user.Role));
    }

    [Authorize]
    [HttpPut("current")]
    public async Task<IActionResult> UpdateCurrentUser(UserUpdateDto updateDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user is null)
            return BadRequest();

        user.Name = updateDto.Name;

         if (!string.IsNullOrEmpty(updateDto.Description))
            user.Description = updateDto.Description;

        //for test
        user.Role = updateDto.Role;
       
        await context.SaveChangesAsync();

        return Ok(new UserResponseDto(
            user.Id, user.Name, "@" + user.Slug, user.Description ?? "", user.RegistryData, user.Email, user.Role));
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