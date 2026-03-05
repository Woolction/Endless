using Backend.API.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Services;
using Backend.API.Dtos;
using Backend.API.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly AuthService authService;
    private readonly IPasswordHasher<User> passwordHasher;

    public AuthController(EndlessContext context, AuthService authService, IPasswordHasher<User> passwordHasher)
    {
        this.context = context;

        this.authService = authService;
        this.passwordHasher = passwordHasher;
    }

    [HttpPost("token")] 
    public async Task<IActionResult> Login([FromBody] AuthRequestDto requestDto)
    {
        User? user = await context.Users.FirstOrDefaultAsync(user => user.Email == requestDto.Email);

        if (user == null)
            return Unauthorized("Password or Email dont correct");

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, requestDto.Password);

        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Password or Email dont correct");

        string token = authService.GenerateToken();

        return Ok(token);
    }
}