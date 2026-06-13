using Application.Features.Channels.Update;
using Application.Features.Rows.Channels;
using Application.Features.Images.Upload;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Db;
using Application.Features.Rows;
using Application.Utilities;
using Domain.Common.Enums;
using System.Text.Json;
using Domain.Entities;
using MediatR;

namespace Application.Features.Channels.Create.One;

public class ChannelCreatingHandler : IRequestHandler<ChannelCreateCommand, Result<ChannelUpdateDto>>
{
    private readonly ILogger<ChannelCreatingHandler> logger;
    private readonly SearchIndexUpsertPublisher indexUpsertPublisher;
    private readonly ImageUploadPublisher publisher;
    private readonly IAppDbContext context;
    private readonly IStorage Storage;


    public ChannelCreatingHandler(IAppDbContext context, SearchIndexUpsertPublisher indexUpsertPublisher, ImageUploadPublisher publisher, ILogger<ChannelCreatingHandler> logger, IStorage Storage)
    {
        this.indexUpsertPublisher = indexUpsertPublisher;
        this.Storage = Storage;
        this.publisher = publisher;
        this.context = context;
        this.logger = logger;
    }
    public async Task<Result<ChannelUpdateDto>> Handle(ChannelCreateCommand cmd, CancellationToken cancellationToken)
    {
        User? user = await context.Users.FindAsync(cmd.UserId, cancellationToken);

        if (user == null)
            return Result<ChannelUpdateDto>.Failure(404, "User not found");

        string slug = cmd.Name.GenerateSlug();

        Channel channel = new()
        {
            Slug = slug,
            Name = cmd.Name,
            CreatedDate = DateTime.UtcNow,
            IsWound = false
        };

        ChannelOwner channelOwner = new()
        {
            OwnerId = cmd.UserId,
            Channel = channel,
            OwnedDate = DateTime.UtcNow,
            OwnerRole = ChannelOwnerRole.Admin
        };

        ChannelSubscription channelSubscription = new()
        {
            Channel = channel,
            SubscriberId = cmd.UserId,
            SubscribedDate = DateTime.UtcNow,
            Notification = false
        };

        context.ChannelSubscriptions.Add(channelSubscription);
        context.ChannelOwners.Add(channelOwner);
        context.Channels.Add(channel);

        await context.SaveChangesAsync();

        var meta = await context.ChannelMetas
            .AsNoTracking()
            .Include(c => c.Image)
            .FirstAsync(c => c
                .ChannelId == channel.Id,
                cancellationToken);

        // publishing to rabbit queue

        string? photoPath = null;

        if (cmd.IconPhoto != null && cmd.IconPhoto.Length != 0)
        {
            photoPath = await Storage.SaveFormFileAsync(
                cmd.IconPhoto, "Images", cancellationToken);

            if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
            {
                await publisher.PublishAsync(new ImageUploadMessage(
                    channel.Id, ImageOwner.Channel, ImageType.Icon, channel.Slug, photoPath), cancellationToken);
            }
        }
        else
        {
            await indexUpsertPublisher.Publish(
                new SearchIndexUpsertMessage(nameof(Channel),
                    JsonSerializer.Serialize(new ChannelSearchIndex(channel, meta, meta.Image))), cancellationToken);
        }

        logger.LogInformation("Channel {ChannelId} created with slug {Slug}",
            channel.Id, slug);

        return Result<ChannelUpdateDto>.Success(201, new ChannelUpdateDto(
            channel.Id, channel.Name,
            "@" + channel.Slug,
            channel.Description ?? "",
            channel.CreatedDate,
            "Created"));
    }
}