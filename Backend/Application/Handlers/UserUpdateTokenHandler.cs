using Application.Commands.Authentications;
using Application.Dtos.Authentications;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces.Services;
using Domain.Interfaces;
using Domain.Entities;

namespace Application.Handlers;

public class UserUpdateTokenHandler
{
    private readonly IAuthService authService;
    private readonly IAppDbContext context;

    public UserUpdateTokenHandler(IAuthService authService, IAppDbContext context)
    {
        this.authService = authService;
        this.context = context;
    }

    public async Task<Result<AuthDto>> Handle(RefreshTokenCommand cmd)
    {
        User? user = await context.Users
            .Include(u => u.RefreshToken)
            .FirstOrDefaultAsync(user =>
                user.RefreshToken != null && user.RefreshToken.Token == cmd.Token);

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