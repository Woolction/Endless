using Application.Channels.Dtos;
using Application.Utilities;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Application.Channels.Create.One;

public class ChannelCreatingHandler : IRequestHandler<ChannelCreateCommand, Result<ChannelDto>>
{
    private readonly ILogger<ChannelCreatingHandler> logger;
    private readonly IAppDbContext context;
    private readonly IR2Service r2Service;


    public ChannelCreatingHandler(IAppDbContext context, ILogger<ChannelCreatingHandler> logger, IR2Service r2Service)
    {
        this.r2Service = r2Service;
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
        };

        if (cmd.AvatarPhoto != null && cmd.AvatarPhoto.Length != 0)
        {
            string photoPath = await r2Service.SaveFormFileAsync(
                cmd.AvatarPhoto, "Images", ".jpeg");

            string photoUrl = await r2Service.SaveImage(photoPath);

            channel.AvatarPhotoUrl = photoUrl;

            File.Delete(photoPath);
        }

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

        try
        {
            await context.SaveChangesAsync();

            logger.LogInformation("Channel {ChannelId} created with slug {Slug}",
                channel.Id, slug);

            return Result<ChannelDto>.Success(201, new ChannelDto(
                channel.Id, channel.Name,
                "@" + channel.Slug,
                channel.Description ?? "",
                channel.CreatedDate,
                channel.AvatarPhotoUrl,
                1, 0, 1, 0, 0));
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error while creating Channel for user {UserId}", cmd.UserId);

            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                return Result<ChannelDto>.Failure(409, "Channel name already exists");

            throw;
        }
    }
}