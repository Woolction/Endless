using Application.Features.Rows.Channels;
using Application.Features.Images.Upload;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Rows;
using Application.Interfaces.Db;
using Application.Utilities;
using Domain.Common.Enums;
using System.Text.Json;
using Domain.Entities;
using MediatR;

namespace Application.Features.Channels.Update;

public class ChannelUpdateHandler : IRequestHandler<ChannelUpdateCommand, Result<ChannelUpdateDto>>
{
    private readonly ILogger<ChannelUpdateHandler> logger;
    private readonly SearchIndexUpsertPublisher indexUpsertPublisher;
    private readonly ImageUploadPublisher publisher;
    private readonly IAppDbContext context;
    private readonly IStorage Storage;

    public ChannelUpdateHandler(IAppDbContext context, SearchIndexUpsertPublisher indexUpsertPublisher, ImageUploadPublisher publisher, ILogger<ChannelUpdateHandler> logger, IStorage Storage)
    {
        this.indexUpsertPublisher = indexUpsertPublisher;
        this.Storage = Storage;
        this.publisher = publisher;
        this.context = context;
        this.logger = logger;
    }

    public async Task<Result<ChannelUpdateDto>> Handle(ChannelUpdateCommand cmd, CancellationToken cancellationToken)
    {
        var channel = await context.Channels
            .Select(channel => new
            {
                c = channel,
                image = channel.ChannelMeta.Image,
                variants = channel.ChannelMeta.Image.Variants,
                dto = new ChannelUpdateDto(
                    channel.Id, channel.Name, "@" + channel.Slug,
                    channel.Description ?? "", channel.CreatedDate, "Processing...")
            })
            .FirstOrDefaultAsync(
                channel => channel.c.Id == cmd.ChannelId,
                cancellationToken);

        if (channel is null || channel.c is null)
            return Result<ChannelUpdateDto>.Failure(404, "Channel not found");

        ChannelOwner? currentOwner = await context.ChannelOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == cmd.UserId &&
                owner.ChannelId == cmd.ChannelId,
                cancellationToken);

        if (currentOwner == null)
        {
            logger.LogWarning("User {UserId} tried to update Channel without permission",
                cmd.UserId);
            return Result<ChannelUpdateDto>.Failure(403, "You doesn't owner the Channel");
        }

        if (currentOwner.OwnerRole != ChannelOwnerRole.Admin)
        {
            logger.LogWarning("User {UserId} tried to update Channel without permission",
                cmd.UserId);
            return Result<ChannelUpdateDto>.Failure(403, "You do not have sufficient rights");
        }

        string slug = cmd.Name.GenerateSlug();

        if (await context.Channels.AnyAsync(channel => channel.Id != cmd.ChannelId &&
            (channel.Name == cmd.Name || channel.Slug == slug), cancellationToken))
            return Result<ChannelUpdateDto>.Failure(409, $"Channel whit name {cmd.Name} hasted");

        string oldName = channel.c.Name;

        // updating
        if (!string.IsNullOrEmpty(cmd.Description))
            channel.c.Description = cmd.Description;

        channel.c.Name = cmd.Name;
        channel.c.Slug = slug;

        string? photoPath = null;

        if (cmd.IconPhoto != null && cmd.IconPhoto.Length > 0)
        {
            photoPath = await Storage.SaveFormFileAsync(
                cmd.IconPhoto, "Images", cancellationToken);
        }

        await context.SaveChangesAsync();

        // publishing to rabbit queue 

        if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
        {
            await publisher.PublishAsync(new ImageUploadMessage(
                channel.c.Id, ImageOwner.Channel, ImageType.Icon, channel.c.Slug, photoPath), cancellationToken);
        }
        else
        {
            await indexUpsertPublisher.Publish(
                new SearchIndexUpsertMessage(nameof(Channel),
                    JsonSerializer.Serialize(new ChannelSearchIndex(channel.c!, channel.image, channel.variants))), cancellationToken);
        }

        logger.LogInformation(
            "Channel {ChannelId} updated successfully. Changed the name from: {OldName} to: {NewName}",
            cmd.ChannelId, oldName, cmd.Name);

        return Result<ChannelUpdateDto>.Success(200, channel.dto);
    }
}