using Application.Features.Rows.Contents;
using Application.Features.Users.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Images;
using Application.Interfaces.Db;
using MediatR;

namespace Application.Features.Users.Choose;

public class UserChooseHandler : IRequestHandler<UserChooseQuery, Result<UserDto>>
{
    private readonly ILogger<UserChooseHandler> logger;
    private readonly IAppDbContext context;
    public UserChooseHandler(IAppDbContext context, ILogger<UserChooseHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<Result<UserDto>> Handle(UserChooseQuery query, CancellationToken cancellationToken)
    {
        var userDto = await context.Users
            .Where(userDto => userDto.Id == query.UserId)
            .Select(userDto => new UserDto(
                userDto.Id, userDto.Name, "@" + userDto.Slug,
                userDto.Description ?? "", userDto.RegistryData, userDto.Email,
                userDto.Role.ToString(),
                new ImageDto(
                    new ImageVariantsDto(
                        userDto.Meta.Image.BaseUrl,
                        userDto.Meta.Image.Variants
                            .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                            .ToList()),
                    userDto.Meta.Image.R,
                    userDto.Meta.Image.G,
                    userDto.Meta.Image.B),
                userDto.TotalLikes, userDto.Comments.Count, userDto.Contents.Count, userDto.Followers.Count,
                userDto.Following.Count, userDto.OwnedChannels.Count, userDto.SubscripedChannels.Count))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (userDto == null)
            return Result<UserDto>.Failure(404, "User not found");

        logger.LogInformation("Returned user {UserId}",
            query.UserId);

        return Result<UserDto>.Success(200, userDto);
    }
}