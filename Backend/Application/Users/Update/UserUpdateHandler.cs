using Application.Users.Dtos;
using Application.Utilities;
using Domain.Interfaces;
using Domain.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Application.Users.Update;

public class UserUpdateHandler : IRequestHandler<UserUpdateCommand, Result<UserDto>>
{
    private readonly ILogger<UserUpdateHandler> logger;
    private readonly IAppDbContext context;
    private readonly IFfmpegService ffmpegService;
    private readonly IR2Service r2Service;

    public UserUpdateHandler(IAppDbContext context, ILogger<UserUpdateHandler> logger, IFfmpegService ffmpegService, IR2Service r2Service)
    {
        this.context = context;
        this.logger = logger;

        this.ffmpegService = ffmpegService;
        this.r2Service = r2Service;
    }
    
    public async Task<Result<UserDto>> Handle(UserUpdateCommand cmd, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Select(user => new
            {
                u = user,
                CommentsCount = user.Comments.Count,
                ContentsCount = user.Contents.Count,
                FollowersCount = user.Followers.Count,
                FollowingCount = user.Following.Count,
                OwnedChannelsCount = user.OwnedChannels.Count,
                SubscripedChannelsCount = user.SubscripedChannels.Count
            })
            .FirstOrDefaultAsync(user => user.u.Id == cmd.UserId, cancellationToken);

        if (user == null || user.u == null)
            return Result<UserDto>.Failure(404, "User not found");

        if (!string.IsNullOrEmpty(cmd.Name))
        {
            user.u.Name = cmd.Name;
            user.u.Slug = cmd.Name.GenerateSlug();
        }

        if (!string.IsNullOrEmpty(cmd.Description))
            user.u.Description = cmd.Description;

        if (cmd.AvatarPhoto != null && cmd.AvatarPhoto.Length != 0)
        {
            string photoPath = await r2Service.SaveFormFileAsync(cmd.AvatarPhoto, "Images", ".jpeg");

            string photoUrl = await r2Service.SaveImage(photoPath);

            user.u.AvatarPhotoUrl = photoUrl;

            File.Delete(photoPath);
        }

        //for test
        user.u.Role = cmd.Role;

        try
        {
            await context.SaveChangesAsync();

            logger.LogInformation("User {UserId} updated", cmd.UserId);

            UserDto userDto = new(
                user.u.Id, user.u.Name, "@" + user.u.Slug,
                user.u.Description ?? "", user.u.RegistryData,
                user.u.Email, user.u.Role.ToString(),
                user.u.AvatarPhotoUrl, user.u.TotalLikes,
                user.CommentsCount, user.ContentsCount,
                user.FollowersCount, user.FollowingCount,
                user.OwnedChannelsCount, user.SubscripedChannelsCount);

            return Result<UserDto>.Success(200, userDto);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error while updating user {UserId}", cmd.UserId);

            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                return Result<UserDto>.Failure(409, $"This name {cmd.Name} already existn");

            throw;
        }
    }
}