using Application.Authentications.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces.Services;
using Domain.Interfaces;
using Domain.Entities;

namespace Application.Authentications.Login;

public class UserLoginHandler
{
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IAuthService authService;
    private readonly IAppDbContext context;

    public UserLoginHandler(IPasswordHasher<User> passwordHasher, IAuthService authService, IAppDbContext context)
    {
        this.passwordHasher = passwordHasher;
        this.authService = authService;
        this.context = context;
    }

    public async Task<Result<AuthDto>> Handle(AuthCreateCommand cmd)
    {
        User? user = await context.Users.FirstOrDefaultAsync(user =>
            user.Email == cmd.Email);

        if (user is null)
            return Result<AuthDto>.Failure(404, "User not found");

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, cmd.Password);

        if (result == PasswordVerificationResult.Failed)
            return Result<AuthDto>.Failure(400, "Dont valid password");

        string[] tokens = await authService.CreateTokenResponse(user);

        if (tokens.Length != 2)
            return Result<AuthDto>.Failure(500, "Token could not be created");

        return Result<AuthDto>.Success(200, new AuthDto(user.Id, tokens[0], tokens[1]));
    }
}