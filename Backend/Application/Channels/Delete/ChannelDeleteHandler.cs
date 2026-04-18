using Application.Channels.Update;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Domain.Common;
using Domain.Interfaces.Repositories;

namespace Application.Channels.Delete;

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
            logger.LogWarning("User {UserId} tried to delete Channel {ChannelId} without permission",
                cmd.UserId, cmd.ChannelId);

            return Result<Null>.Failure(403, "You doesn't owner the Channel");
        }

        if (currentOwner.OwnerRole != ChannelOwnerRole.Admin)
        {
            logger.LogWarning("Delete denied for user {UserId} on Channel {ChannelId}",
                cmd.UserId, cmd.ChannelId);

            return Result<Null>.Failure(403, "You do not have sufficient rights");
        }

        Channel? Channel = await context.Channels.FindAsync(
            cmd.ChannelId, cancellationToken);

        if (Channel == null)
            return Result<Null>.Failure(404, "Channel not found");

        context.Channels.Remove(Channel);

        await context.SaveChangesAsync();

        await channelRepository.DeleteSearchIndex(cmd.ChannelId, cancellationToken);

        logger.LogInformation("Channel {ChannelId} deleted", cmd.ChannelId);

        return Result<Null>.Success(204, new Null());
    }
}