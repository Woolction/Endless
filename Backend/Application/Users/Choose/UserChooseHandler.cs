using Application.Users.Dtos;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Users.Choose;

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
        UserDto? userDto = await context.Users
            .Where(user => user.Id == query.UserId)
            .Select(user => new UserDto(
                user.Id, user.Name, "@" + user.Slug,
                user.Description ?? "", user.RegistryData, user.Email,
                user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                user.Comments.Count, user.Contents.Count, user.Followers.Count,
                user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (userDto == null)
            return Result<UserDto>.Failure(404, "User not found");

        logger.LogInformation("Returned user {UserId}",
            query.UserId);

        return Result<UserDto>.Success(200, userDto);
    }
}