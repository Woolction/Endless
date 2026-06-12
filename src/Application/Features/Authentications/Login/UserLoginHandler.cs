using Application.Features.Authentications.Dtos;
using Application.Features.Rows.Contents;
using Application.Features.Users.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Application.Features.Imagess;
using Application.Features.Dtos;
using Application.Interfaces.Db;
using Domain.Entities;
using MediatR;

namespace Application.Features.Authentications.Login;

public class UserLoginHandler : IRequestHandler<AuthCreateCommand, Result<AuthDto>>
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

    public async Task<Result<AuthDto>> Handle(AuthCreateCommand cmd, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Select(user => new
            {
                u = user,
                dto = new UserDto(
                user.Id, user.Name, "@" + user.Slug,
                user.Description ?? "", user.RegistryData, user.Email,
                user.Role.ToString(), new PhotoDto(
                    new ImageVariants(
                        user.UserMeta.IconBase,
                        user.UserMeta.Small,
                        user.UserMeta.Medium,
                        user.UserMeta.Large),
                    user.UserMeta.R,
                    user.UserMeta.G,
                    user.UserMeta.B), user.TotalLikes,
                0, 0, 0, 0, 0, 0)
            })
            .FirstOrDefaultAsync(user =>
                user.u.Email == cmd.Email, cancellationToken);

        if (user == null || user.u == null)
            return Result<AuthDto>.Failure(404, "User not found");

        var result = passwordHasher.VerifyHashedPassword(user.u, user.u.PasswordHash, cmd.Password);

        if (result == PasswordVerificationResult.Failed)
            return Result<AuthDto>.Failure(400, "Dont valid password");

        string[] tokens = await authService.CreateTokenResponse(user.u);

        if (tokens.Length != 2)
            return Result<AuthDto>.Failure(500, "Token could not be created");

        return Result<AuthDto>.Success(200, new AuthDto(user.dto, tokens[0], tokens[1]));
    }
}