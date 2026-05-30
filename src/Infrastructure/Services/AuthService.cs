using Microsoft.Extensions.Configuration;
using Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Application.Interfaces.Db;
using System.Security.Claims;
using Domain.Entities;
using System.Text;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext context;

    private readonly IConfiguration jwtSettings;
    private readonly IRandomService randomService;
    private readonly SymmetricSecurityKey securetyKey;

    private const int refreshTokenExpires = 30;

    public AuthService(IAppDbContext context, IConfiguration configuration, IRandomService randomService)
    {
        this.context = context;
        this.randomService = randomService;

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
        string token = randomService.GenerateToken(32);

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