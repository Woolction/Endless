using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Backend.API.Data.Context;
using Backend.API.Data.Model;
using System.Security.Claims;
using Backend.API.Dtos;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.API.Services;

public class AuthService : IAuthService
{
    private readonly EndlessContext context;

    private readonly IConfiguration jwtSettings;
    private readonly SymmetricSecurityKey securityKey;
    private readonly IPasswordHasher<User> passwordHasher;

    public AuthService(EndlessContext context, IConfiguration configuration, IPasswordHasher<User> passwordHasher)
    {
        this.context = context;

        jwtSettings = configuration.GetSection("JwtSettings");
        byte[] key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        securityKey = new SymmetricSecurityKey(key);

        this.passwordHasher = passwordHasher;
    }

    public async Task<User?> RegistryAsync(AuthRequestDto requestDto)
    {
        if (await context.Users.AnyAsync(users => users.Email == requestDto.Email))
            return null;

        User user = new() { Email = requestDto.Email };
        user.PasswordHash = passwordHasher.HashPassword(user, requestDto.Password);

        context.Users.Add(user);

        await context.SaveChangesAsync();

        return user;
    }
    public async Task<string?> LoginAsync(AuthRequestDto requestDto)
    {
        User? user = await context.Users.FirstOrDefaultAsync(user => user.Email == requestDto.Email);

        if (user is null)
            return null;

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, requestDto.Password);

        if (result == PasswordVerificationResult.Failed)
            return null;

        return GenerateJWTToken(user);
    }

    public string GenerateJWTToken(User user)
    {
        Claim[] claims = [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        ];

        JwtSecurityToken token = new(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
            signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512)
            );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}