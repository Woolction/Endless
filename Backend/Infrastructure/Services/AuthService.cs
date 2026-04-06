using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Domain.Interfaces.Services;
using System.Security.Claims;
using Infrastructure.Context;
using Domain.Entities;
using System.Text;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly EndlessContext context;

    private readonly IConfiguration jwtSettings;
    private readonly SymmetricSecurityKey securetyKey;

    private const int refreshTokenExpires = 30;

    public AuthService(EndlessContext context, IConfiguration configuration)
    {
        this.context = context;

        //jwt configuration and get security key
        jwtSettings = configuration.GetSection("JwtSettings");
        byte[] key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        securetyKey = new SymmetricSecurityKey(key);
    }

    public async Task<string[]> CreateTokenResponse(User user)
    {
        return [GenerateJWTToken(user), await GenerateRefreshToken(user)];
    }

    private string GenerateJWTToken(User user)
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
            signingCredentials: new SigningCredentials(securetyKey, SecurityAlgorithms.HmacSha512)
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private async Task<string> GenerateRefreshToken(User user)
    {
        //generate refresh token
        byte[] randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        string token = Convert.ToBase64String(randomNumber);

        //update the user token 
        user.RefreshToken = new()
        {
            Token = token,
            ValidityPeriod = DateTime.UtcNow.AddDays(refreshTokenExpires)
        };

        await context.SaveChangesAsync();

        return token;
    }
}