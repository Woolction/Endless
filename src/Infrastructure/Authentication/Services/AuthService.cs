using Microsoft.Extensions.Configuration;
using Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Application.Interfaces.Db;
using System.Security.Claims;
using Domain.Entities;
using System.Text;
using Microsoft.Extensions.Options;

namespace Authentication.Services;

public class AuthService : IAuthService
{
    private readonly JwtOptions jwtOptions;
    private readonly IRandomService randomService;
    private readonly SymmetricSecurityKey securetyKey;

    public AuthService(IOptions<JwtOptions> options, IRandomService randomService)
    {
        this.randomService = randomService;

        //jwt configuration and get security key

        jwtOptions = options.Value;

        byte[] key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);
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
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpireMinutes),
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
            ValidityPeriod = DateTime.UtcNow.AddDays(jwtOptions.ExpireMinutes)
        };

        return token;
    }
}