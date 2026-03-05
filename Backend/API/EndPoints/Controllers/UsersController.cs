using Microsoft.AspNetCore.Identity; 
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Model;
using Backend.API.Dtos;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly EndlessContext context;
    private readonly IPasswordHasher<User> passwordHasher;
    public UsersController(EndlessContext context, IPasswordHasher<User> passwordHasher)
    {
        this.context = context;
        this.passwordHasher = passwordHasher;
    }
    
    [HttpPost] 
    public async Task<IActionResult> UserRegistry([FromBody] AuthRequestDto requestDto)
    {
        if (await context.Users.AnyAsync(users => users.Email == requestDto.Email))
            return BadRequest("");

        User user = new() { Email = requestDto.Email };
        user.PasswordHash = passwordHasher.HashPassword(user, requestDto.Password);

        await context.Users.AddAsync(user);

        await context.SaveChangesAsync();

        return Ok("User Registred");
    }
}