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
    [Authorize]
    public async Task<IActionResult> GetUser()
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user is null)
            return NotFound();

        return Ok(new UserResponseDto(user.Name, user.Email, user.Role));
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateUser(UserUpdateDto updateDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user is null)
            return BadRequest();

        user.Name = updateDto.Name;

        //for test
        user.Role = updateDto.Role;

        await context.SaveChangesAsync();

        return Ok(new UserResponseDto(user.Name, user.Email, user.Role));
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteUser()
    {
        Guid currentUserId = this.GetIDFromClaim();

        await context.Users.Where(user => user.Id == currentUserId).ExecuteDeleteAsync();

        this.DeleteTokensInCookies();

        return NoContent();
    }
}