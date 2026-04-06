using Application.Commands.Authentications;
using Contracts.Dtos.Authentications;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Domain.Interfaces.Services;
using Domain.Entities;

namespace Application.Handlers;

public class UserLoginHandler
{
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IUserRepository userRepository;
    private readonly IAuthService authService;

    public UserLoginHandler(IPasswordHasher<User> passwordHasher, IUserRepository userRepository, IAuthService authService)
    {
        this.passwordHasher = passwordHasher;
        this.userRepository = userRepository;
        this.authService = authService;
    }

    public async Task<Result<AuthDto>> Handle(AuthCreateCommand cmd)
    {
        User? user = await userRepository.GetUserByEmail(cmd.Email);

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