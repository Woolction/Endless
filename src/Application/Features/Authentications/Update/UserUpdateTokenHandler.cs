using Application.Features.Authentications.Dtos;
using Application.Features.Users.Update;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Db;
using MediatR;
using Application.Features.Users.Dtos;
using Application.Features.Images;

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
            .Where(user => user.RefreshToken != null && user.RefreshToken.Token == cmd.Token)
            .Select(user => new
            {
                u = user,
                dto = new UserDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(),
                    new ImageDto(
                        user.Meta.Image.BaseUrl,
                        user.Meta.Image.Variants
                            .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                            .ToList(),
                        user.Meta.Image.R,
                        user.Meta.Image.G,
                        user.Meta.Image.B),
                    user.TotalLikes, user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null || user.u == null)
            return Result<AuthDto>.Failure(404, $"User by Token: {cmd.Token} not found");

        if (user.u.RefreshToken!.ValidityPeriod <= DateTime.UtcNow)
            return Result<AuthDto>.Failure(400, "Token has expired");

        string[] tokens = await authService.CreateTokenResponse(user.u);

        if (tokens.Length != 2)
            return Result<AuthDto>.Failure(500, "Token could not be created");

        await context.SaveChangesAsync();

        return Result<AuthDto>.Success(200, new AuthDto(user.dto, tokens[0], tokens[1]));
    }
}