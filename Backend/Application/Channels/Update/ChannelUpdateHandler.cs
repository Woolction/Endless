using Application.Channels.Dtos;
using Application.Utilities;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Application.Channels.Update;

public class ChannelUpdateHandler : IRequestHandler<ChannelUpdateCommand, Result<ChannelDto>>
{
    private readonly ILogger<ChannelUpdateHandler> logger;
    private readonly IAppDbContext context;

    public ChannelUpdateHandler(IAppDbContext context, ILogger<ChannelUpdateHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }
    
    public async Task<Result<ChannelDto>> Handle(ChannelUpdateCommand cmd, CancellationToken cancellationToken)
    {
        var channel = await context.Channels
            .Select(channel => new {
                d = channel,
                SubscribersCount = channel.Subscribers.Count,
                ContentsCount = channel.Contents.Count,
                OwnersCount = channel.Owners.Count})
            .FirstOrDefaultAsync(
                channel => channel.d.Id == cmd.ChannelId,
                cancellationToken);

        if (channel is null || channel.d is null)
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

        if (!string.IsNullOrEmpty(cmd.Description))
            channel.d.Description = cmd.Description;

        string slug = cmd.Name.GenerateSlug();

        if (await context.Channels.AnyAsync(channel => channel.Name == cmd.Name || channel.Slug == slug, cancellationToken))
            return Result<ChannelDto>.Failure(409, $"Channel whit name {cmd.Name} hasted");

        string oldName = channel.d.Name;

        //Updating
        channel.d.Name = cmd.Name;
        channel.d.Slug = slug;

        try
        {
            ChannelDto channelDto = new(
                channel.d.Id, channel.d.Name, "@" + channel.d.Slug,
                channel.d.Description ?? "", channel.d.CreatedDate,
                channel.d.AvatarPhotoUrl, channel.SubscribersCount,
                channel.ContentsCount, channel.OwnersCount,
                channel.d.TotalLikes, channel.d.TotalViews);

            await context.SaveChangesAsync();

            logger.LogInformation(
                "Channel {ChannelId} updated successfully. Changed the name from: {OldName} to: {NewName}",
                cmd.ChannelId, oldName, cmd.Name);

            return Result<ChannelDto>.Success(200, channelDto);
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