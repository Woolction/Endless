using Application.Features.Dtos;
using Application.Features.Channels.Dtos;
using Application.Features.Icon.Upload;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Db;
using Application.Utilities;
using Domain.Rows.Contents;
using Domain.Common.Enums;
using Domain.Entities;
using MediatR;
using Npgsql;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Rows.Icon.Upload;

namespace Application.Features.Channels.Update;

public class ChannelUpdateHandler : IRequestHandler<ChannelUpdateCommand, Result<ChannelDto>>
{
    private readonly ILogger<ChannelUpdateHandler> logger;
    private readonly IChannelRepository repository;
    private readonly IconUploadPublisher publisher;
    private readonly IAppDbContext context;
    private readonly IR2Service r2Service;

    public ChannelUpdateHandler(IAppDbContext context, IChannelRepository repository, IconUploadPublisher publisher, ILogger<ChannelUpdateHandler> logger, IR2Service r2Service)
    {
        this.repository = repository;
        this.r2Service = r2Service;
        this.publisher = publisher;
        this.context = context;
        this.logger = logger;
    }

    public async Task<Result<ChannelDto>> Handle(ChannelUpdateCommand cmd, CancellationToken cancellationToken)
    {
        var channel = await context.Channels
            .Select(channel => new
            {
                c = channel,
                meta = channel.ChannelMeta,
                dto = new ChannelDto(
                channel.Id, channel.Name, "@" + channel.Slug,
                channel.Description ?? "", channel.CreatedDate,
                new PhotoDto(
                    new PhotoVariants(
                        channel.ChannelMeta.IconBase,
                        channel.ChannelMeta.Small,
                        channel.ChannelMeta.Medium,
                        channel.ChannelMeta.Large),
                    channel.ChannelMeta.R,
                    channel.ChannelMeta.G,
                    channel.ChannelMeta.B),
                0, 0, 0, channel.TotalLikes, channel.TotalViews)
            })
            .FirstOrDefaultAsync(
                channel => channel.c.Id == cmd.ChannelId,
                cancellationToken);

        if (channel is null || channel.c is null)
            return Result<ChannelDto>.Failure(404, "Channel not found");

        ChannelOwner? currentOwner = await context.ChannelOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == cmd.UserId &&
                owner.ChannelId == cmd.ChannelId,
                cancellationToken);

        if (currentOwner == null)
        {
            logger.LogWarning("User {UserId} tried to update Channel without permission",
                cmd.UserId);
            return Result<ChannelDto>.Failure(403, "You doesn't owner the Channel");
        }

        if (currentOwner.OwnerRole != ChannelOwnerRole.Admin)
        {
            logger.LogWarning("User {UserId} tried to update Channel without permission",
                cmd.UserId);
            return Result<ChannelDto>.Failure(403, "You do not have sufficient rights");
        }

        string slug = cmd.Name.GenerateSlug();

        if (await context.Channels.AnyAsync(channel => channel.Id != cmd.ChannelId &&
            (channel.Name == cmd.Name || channel.Slug == slug), cancellationToken))
            return Result<ChannelDto>.Failure(409, $"Channel whit name {cmd.Name} hasted");

        string oldName = channel.c.Name;

        // updating
        if (!string.IsNullOrEmpty(cmd.Description))
            channel.c.Description = cmd.Description;

        channel.c.Name = cmd.Name;
        channel.c.Slug = slug;

        string? photoPath = null;

        if (cmd.IconPhoto != null && cmd.IconPhoto.Length > 0)
        {
            photoPath = await r2Service.SaveFormFileAsync(
                cmd.IconPhoto, "Images", cancellationToken);

            // delete old data

            if (Directory.Exists(channel.meta.IconBase))
            {
                Directory.Delete(channel.meta.IconBase, true);
            }
        } 

        await context.SaveChangesAsync();

        // publishing to rabbit queue 

        if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
        {
            await publisher.PublishAsync(new IconUploadMessage(
                channel.c.Id, IconType.Channel, channel.c.Slug, photoPath), cancellationToken);
        }
        else
        {
            await repository.CreateSearchIndex(
                channel.c, channel.meta, cancellationToken);
        }

        logger.LogInformation(
            "Channel {ChannelId} updated successfully. Changed the name from: {OldName} to: {NewName}",
            cmd.ChannelId, oldName, cmd.Name);

        return Result<ChannelDto>.Success(200, channel.dto);
    }
}