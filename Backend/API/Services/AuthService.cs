using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using System.Security.Claims;
using Backend.API.Dtos;
using System.Text;
using Npgsql;

namespace Backend.API.Services;

public class AuthService : IAuthService
{
    private readonly EndlessContext context;

    private readonly IConfiguration jwtSettings;
    private readonly SymmetricSecurityKey securityKey;
    private readonly IPasswordHasher<User> passwordHasher;

    private const int refreshTokenExpires = 30;

    public AuthService(EndlessContext context, IConfiguration configuration, IPasswordHasher<User> passwordHasher)
    {
        this.context = context;

        //jwt configuration and get security key
        jwtSettings = configuration.GetSection("JwtSettings");
        byte[] key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        securityKey = new SymmetricSecurityKey(key);

        this.passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto?> RegistryAsync(AuthRequestDto requestDto)
    {
        if (await context.Users.AnyAsync(users => users.Email == requestDto.Email))
            return null;

        User user = new()
        {
            Email = requestDto.Email,
            RegistryData = DateTime.UtcNow,
        };
        user.PasswordHash = passwordHasher.HashPassword(user, requestDto.Password);

        if (!string.IsNullOrEmpty(requestDto.Name))
        {
            user.Name = requestDto.Name;
            user.Slug = requestDto.Name.GenerateSlug();
        }

        context.UserVectors.AddRange(await context.Genres
            .Select(genre => new UserGenreVector()
            {
                User = user,
                GenreId = genre.Id
            })
            .AsNoTracking()
            .ToListAsync()
        );

        context.Users.Add(user);

        try
        {
            await context.SaveChangesAsync();

            return await CraeteTokenResponse(user);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                return null;

            throw;
        }
    }
    public async Task<AuthResponseDto?> LoginAsync(AuthRequestDto requestDto)
    {
        User? user = await context.Users.FirstOrDefaultAsync(user => user.Email == requestDto.Email);

        if (user is null)
            return null;

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, requestDto.Password);

        if (result == PasswordVerificationResult.Failed)
            return null;

        return await CraeteTokenResponse(user);
    }
    public async Task<AuthResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto requestDto)
    {
        User? user = await ValidateRefreshToken(requestDto);

        if (user is null)
            return null;

        return await CraeteTokenResponse(user);
    }

    #region GenerateTokens
    private async Task<AuthResponseDto> CraeteTokenResponse(User user)
    {
        return new AuthResponseDto(GenerateJWTToken(user), await GenerateRefreshToken(user));
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
            signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512)
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
    #endregion +----------+

    private async Task<User?> ValidateRefreshToken(RefreshTokenRequestDto requestDto)
    {
        if (string.IsNullOrEmpty(requestDto.Token))
            return null;
            
        User? user = await context.Users.Include(u => u.RefreshToken)
                        .FirstOrDefaultAsync(user => user.RefreshToken!.Token == requestDto.Token);

        if (user is not null)
        {
            RefreshToken userRefreshToken = user.RefreshToken!;

            if (userRefreshToken.ValidityPeriod >= DateTime.UtcNow)
                return user;
        }

        return null;
    }
}