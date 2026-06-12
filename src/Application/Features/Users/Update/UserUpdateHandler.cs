using Application.Features.Rows.Contents;
using Application.Features.Image.Upload;
using Application.Features.Users.Dtos;
using Application.Features.Rows.Users;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Images;
using Application.Features.Dtos;
using Application.Features.Rows;
using Application.Interfaces.Db;
using Application.Utilities;
using Domain.Common.Enums;
using System.Text.Json;
using Domain.Entities;
using MediatR;

namespace Application.Features.Users.Update;

public class UserUpdateHandler : IRequestHandler<UserUpdateCommand, Result<UserDto>>
{
    private readonly SearchIndexUpsertPublisher indexUpsertPublisher;
    private readonly ILogger<UserUpdateHandler> logger;
    private readonly ImageUploadPublisher publisher;
    private readonly IAppDbContext context;
    private readonly IStorage Storage;

    public UserUpdateHandler(IAppDbContext context, ILogger<UserUpdateHandler> logger, SearchIndexUpsertPublisher indexUpsertPublisher, ImageUploadPublisher publisher, IStorage Storage)
    {
        this.context = context;
        this.logger = logger;

        this.indexUpsertPublisher = indexUpsertPublisher;
        this.publisher = publisher;
        this.Storage = Storage;
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
                    new ImageVariants(
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
            photoPath = await Storage.SaveFormFileAsync(
                cmd.IconPhoto, "Images", cancellationToken);
        }

        await context.SaveChangesAsync();

        // publishing to rabbit queue

        if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
        {
            await publisher.PublishAsync(new ImageUploadMessage(
                cmd.UserId, IconType.User, user.u.Slug, photoPath), cancellationToken);
        }
        else
        {
            await indexUpsertPublisher.Publish(
                new SearchIndexUpsertMessage(nameof(User),
                    JsonSerializer.Serialize(new UserSearchIndex(user.u, user.meta))), cancellationToken);
        }

        logger.LogInformation("User {UserId} updated", cmd.UserId);

        return Result<UserDto>.Success(200, user.dto);
    }
}