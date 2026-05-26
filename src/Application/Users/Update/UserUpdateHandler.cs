using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Common.Interfaces.Services;
using Application.Users.Dtos;
using Application.Utilities;
using Domain.Common.Interfaces.Db;
using MediatR;
using Npgsql;
using Domain.Rows.Contents;
using Application.Dtos;
using Domain.Common.Interfaces.Repositories;
using Domain.Rows.Icon.Upload;
using Domain.Common.Enums;
using Application.Icon.Upload;

namespace Application.Users.Update;

public class UserUpdateHandler : IRequestHandler<UserUpdateCommand, Result<UserDto>>
{
    private readonly ILogger<UserUpdateHandler> logger;
    private readonly IUserRepository userRepository;
    private readonly IconUploadPublisher publisher;
    private readonly IAppDbContext context;
    private readonly IR2Service r2Service;

    public UserUpdateHandler(IAppDbContext context, ILogger<UserUpdateHandler> logger, IUserRepository userRepository, IconUploadPublisher publisher, IR2Service r2Service)
    {
        this.context = context;
        this.logger = logger;

        this.userRepository = userRepository;
        this.publisher = publisher;
        this.r2Service = r2Service;
    }

    public async Task<Result<UserDto>> Handle(UserUpdateCommand cmd, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Select(user => new
            {
                u = user,
                meta = user.UserMeta,
                dto = new UserDto(
                user.Id, user.Name, "@" + user.Slug,
                user.Description ?? "", user.RegistryData,
                user.Email, user.Role.ToString(),
                new PhotoDto(
                    new PhotoVariants(
                        user.UserMeta.IconBase,
                        user.UserMeta.Small,
                        user.UserMeta.Medium,
                        user.UserMeta.Large),
                    user.UserMeta.R,
                    user.UserMeta.G,
                    user.UserMeta.B), user.TotalLikes,
                user.Comments.Count, user.Contents.Count,
                user.Followers.Count, user.Following.Count,
                user.OwnedChannels.Count, user.SubscripedChannels.Count)
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

        // for test
        user.u.Role = cmd.Role;


        string? photoPath = null;

        if (cmd.IconPhoto != null && cmd.IconPhoto.Length != 0)
        {
            photoPath = await r2Service.SaveFormFileAsync(
                cmd.IconPhoto, "Images", cancellationToken);

            // delete old data

            if (Directory.Exists(user.meta.IconBase))
            {
                Directory.Delete(user.meta.IconBase, true);
            }
        }

        await context.SaveChangesAsync();

        // publishing to rabbit queue

        if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
        {
            await publisher.PublishAsync(new IconUploadMessage(
                cmd.UserId, IconType.User, user.u.Slug, photoPath), cancellationToken);
        }
        else
        {
            await userRepository.CreateSearchIndex(
                user.u, user.meta, cancellationToken);
        }

        logger.LogInformation("User {UserId} updated", cmd.UserId);

        return Result<UserDto>.Success(200, user.dto);
    }
}