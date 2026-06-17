using Application.Features.Channels.Update;
using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Db;
using Domain.Entities;
using Domain.Common.Enums;
using MediatR;

namespace Application.Features.Channels.Delete;

public class ChannelDeleteHandler : IRequestHandler<ChannelDeleteCommand, Result<Null>>
{
    private readonly ILogger<ChannelUpdateHandler> logger;
    private readonly IChannelRepository channelRepository;
    private readonly IAppDbContext context;

    public ChannelDeleteHandler(IAppDbContext context, IChannelRepository channelRepository, ILogger<ChannelUpdateHandler> logger)
    {
        this.channelRepository = channelRepository;
        this.context = context;
        this.logger = logger;
    }

    public async Task<Result<Null>> Handle(ChannelDeleteCommand cmd, CancellationToken cancellationToken)
    {
        ChannelOwner? currentOwner = await context.ChannelOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == cmd.UserId &&
                owner.ChannelId == cmd.ChannelId,
                cancellationToken);

        if (currentOwner == null)
        {
            logger.LogWarning("User {UserId} tried to delete channel {ChannelId} without permission",
                cmd.UserId, cmd.ChannelId);

            return Result<Null>.Failure(403, "You doesn't owner the channel");
        }

        if (currentOwner.OwnerRole != ChannelOwnerRole.Admin)
        {
            logger.LogWarning("Delete denied for user {UserId} on channel {ChannelId}",
                cmd.UserId, cmd.ChannelId);

            return Result<Null>.Failure(403, "You do not have sufficient rights");
        }

        var channel = await context.Channels
            .Where(channel => channel.Id == cmd.ChannelId)
            .Select(channel => new { iconsPath = channel.ChannelMeta.Image.BaseUrl, channel })
            .FirstOrDefaultAsync(cancellationToken);

        if (channel == null)
            return Result<Null>.Failure(404, "channel not found");

        if (!string.IsNullOrEmpty(channel.iconsPath) && Directory.Exists(channel.iconsPath))
            Directory.Delete(channel.iconsPath, true);

        context.Channels.Remove(channel.channel);

        await context.SaveChangesAsync();

        await channelRepository.DeleteSearchIndex(cmd.ChannelId, cancellationToken);

        logger.LogInformation("channel {ChannelId} deleted", cmd.ChannelId);

        return Result<Null>.Success(204, new Null());
    }
}