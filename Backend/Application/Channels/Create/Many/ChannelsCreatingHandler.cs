using Application.Channels.Dtos;
using Application.Utilities;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Application.Channels.Create.Many;

public class ChannelsCreatingHandler : IRequestHandler<ChannelsCreateCommand, Result<ChannelDto[]>>
{
    private readonly IAppDbContext context;

    public ChannelsCreatingHandler(IAppDbContext context)
    {
        this.context = context;
    }

    public async Task<Result<ChannelDto[]>> Handle(ChannelsCreateCommand cmd, CancellationToken cancellationToken)
    {
        User? user = await context.Users.FindAsync(cmd.UserId);

        if (user == null)
            Result<ChannelDto[]>.Failure(404, "User not found");

        List<Channel> channels = new();
        List<ChannelOwner> channelOwners = new();
        List<ChannelSubscription> channelSubscriptions = new();

        for (int i = 0; i < cmd.Count; i++)
        {
            string name = Guid.CreateVersion7().ToString();

            string slug = name.GenerateSlug();

            Channel channel = new()
            {
                Slug = slug,
                Name = name,
                CreatedDate = DateTime.UtcNow,
            };

            channels.Add(channel);
            channelOwners.Add(new ChannelOwner()
            {
                OwnerId = cmd.UserId!.Value,
                Channel = channel,
                OwnedDate = DateTime.UtcNow,
                OwnerRole = ChannelOwnerRole.Admin
            });
            channelSubscriptions.Add(new ChannelSubscription()
            {
                Channel = channel,
                SubscriberId = cmd.UserId!.Value,
                SubscribedDate = DateTime.UtcNow,
                Notification = false
            });
        }

        context.Channels.AddRange(channels);
        context.ChannelOwners.AddRange(channelOwners);
        context.ChannelSubscriptions.AddRange(channelSubscriptions);

        try
        {
            await context.SaveChangesAsync();

            return Result<ChannelDto[]>.Success(201, channels.Select(c =>
                new ChannelDto(
                    c.Id, c.Name, "@" + c.Slug,
                    c.Description ?? "", c.CreatedDate,
                    c.AvatarPhotoUrl, 1, 0, 0, 0, 0)).ToArray());
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                Result<ChannelDto[]>.Failure(409, "User name already exists");

            throw;
        }
    }
}