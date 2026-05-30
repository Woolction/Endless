using Application.Features.Dtos;
using Application.Features.Channels.Dtos;
using Application.Features.Icon.Upload;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Db;
using Application.Utilities;
using Application.Features.Rows.Contents;
using Domain.Common.Enums;
using Domain.Entities;
using MediatR;
using Npgsql;

namespace Application.Features.Channels.Create.One;

public class ChannelCreatingHandler : IRequestHandler<ChannelCreateCommand, Result<ChannelDto>>
{
    private readonly ILogger<ChannelCreatingHandler> logger;
    private readonly IChannelRepository channelRepository;
    private readonly IconUploadPublisher publisher;
    private readonly IAppDbContext context;
    private readonly IR2Service r2Service;


    public ChannelCreatingHandler(IAppDbContext context, IChannelRepository channelRepository, IconUploadPublisher publisher, ILogger<ChannelCreatingHandler> logger, IR2Service r2Service)
    {
        this.channelRepository = channelRepository;
        this.r2Service = r2Service;
        this.publisher = publisher;
        this.context = context;
        this.logger = logger;
    }
    public async Task<Result<ChannelDto>> Handle(ChannelCreateCommand cmd, CancellationToken cancellationToken)
    {
        User? user = await context.Users.FindAsync(cmd.UserId, cancellationToken);

        if (user == null)
            return Result<ChannelDto>.Failure(404, "User not found");

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
            .FirstAsync(c => c
                .ChannelId == channel.Id,
                cancellationToken);

        // publishing to rabbit queue

        string? photoPath = null;

        if (cmd.IconPhoto != null && cmd.IconPhoto.Length != 0)
        {
            photoPath = await r2Service.SaveFormFileAsync(
                cmd.IconPhoto, "Images", cancellationToken);

            if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
            {
                await publisher.PublishAsync(new IconUploadMessage(
                    channel.Id, IconType.Channel, channel.Slug, photoPath), cancellationToken);
            }
        }
        else
        {
            await channelRepository.CreateSearchIndex(
                channel, meta, cancellationToken);
        }

        logger.LogInformation("Channel {ChannelId} created with slug {Slug}",
            channel.Id, slug);

        return Result<ChannelDto>.Success(201, new ChannelDto(
            channel.Id, channel.Name,
            "@" + channel.Slug,
            channel.Description ?? "",
            channel.CreatedDate,
            new PhotoDto(
                new PhotoVariants(
                    meta.IconBase,
                    meta.Small,
                    meta.Medium,
                    meta.Large),
                meta.R,
                meta.G,
                meta.B),
            1, 0, 1, 0, 0));
    }
}