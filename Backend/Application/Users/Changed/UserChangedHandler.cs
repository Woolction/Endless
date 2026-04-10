using Application.Users.Dtos;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Users.Changed;

public class UserChangedHandler : IRequestHandler<UserChangedQuery, Result<UserDto>>
{
    private readonly ILogger<UserChangedHandler> logger;
    private readonly IAppDbContext context;
    public UserChangedHandler(IAppDbContext context, ILogger<UserChangedHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }
    
    public async Task<Result<UserDto>> Handle(UserChangedQuery query, CancellationToken cancellationToken)
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