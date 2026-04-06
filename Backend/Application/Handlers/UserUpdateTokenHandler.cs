using Application.Commands.Authentications;
using Contracts.Dtos.Authentications;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Entities;

namespace Application.Handlers;

public class UserUpdateTokenHandler
{
    private readonly IUserRepository userRepository;
    private readonly IAuthService authService;

    public UserUpdateTokenHandler(IUserRepository userRepository, IAuthService authService)
    {
        this.userRepository = userRepository;
        this.authService = authService;
    }

    public async Task<Result<AuthDto>> Handle(RefreshTokenCommand cmd)
    {
        User? user = await userRepository.GetUserByRefreshToken(cmd.Token);

        if (user == null)
            return Result<AuthDto>.Failure(404, $"User by Token: {cmd.Token} not found");

        if (user.RefreshToken!.ValidityPeriod <= DateTime.UtcNow)
            return Result<AuthDto>.Failure(400, "Token has expired");

        string[] tokens = await authService.CreateTokenResponse(user);

        if (tokens.Length != 2)
            return Result<AuthDto>.Failure(500, "Token could not be created");

        return Result<AuthDto>.Success(200, new AuthDto(user.Id, tokens[0], tokens[1]));
    }
}