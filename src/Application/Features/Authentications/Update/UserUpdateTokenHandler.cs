using Application.Features.Authentications.Dtos;
using Application.Features.Users.Update;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Db;
using MediatR;

namespace Application.Features.Authentications.Update;

public class UserUpdateTokenHandler : IRequestHandler<RefreshTokenCommand, Result<AuthDto>>
{
    private readonly IAuthService authService;
    private readonly IAppDbContext context;

    public UserUpdateTokenHandler(IAuthService authService, IAppDbContext context)
    {
        this.authService = authService;
        this.context = context;
    }

    public async Task<Result<AuthDto>> Handle(RefreshTokenCommand cmd, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Select(user => new
            {
                u = user,
                dto = new UserUpdateDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), "Token Updated")
            })
            .FirstOrDefaultAsync(user =>
                user.u.RefreshToken != null && user.u.RefreshToken.Token == cmd.Token, cancellationToken);

        if (user == null || user.u == null)
            return Result<AuthDto>.Failure(404, $"User by Token: {cmd.Token} not found");

        if (user.u.RefreshToken!.ValidityPeriod <= DateTime.UtcNow)
            return Result<AuthDto>.Failure(400, "Token has expired");

        string[] tokens = await authService.CreateTokenResponse(user.u);

        if (tokens.Length != 2)
            return Result<AuthDto>.Failure(500, "Token could not be created");

        return Result<AuthDto>.Success(200, new AuthDto(user.dto, tokens[0], tokens[1]));
    }
}