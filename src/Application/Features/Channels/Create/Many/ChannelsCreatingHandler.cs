using Application.Features.Rows.Contents;
using Application.Features.Channels.Dtos;
using Application.Features.Rows.Channels;
using Microsoft.EntityFrameworkCore;
using Application.Features.Imagess;
using Application.Features.Dtos;
using Application.Features.Rows;
using Application.Interfaces.Db;
using Application.Utilities;
using Domain.Common.Enums;
using System.Text.Json;
using Domain.Entities;
using MediatR;
using Npgsql;

namespace Application.Features.Channels.Create.Many;

public class ChannelsCreatingHandler : IRequestHandler<ChannelsCreateCommand, Result<ChannelDto[]>>
{
    private readonly SearchIndexUpsertPublisher indexUpsertPublisher;
    private readonly IAppDbContext context;

    public ChannelsCreatingHandler(IAppDbContext context, SearchIndexUpsertPublisher indexUpsertPublisher)
    {
        this.indexUpsertPublisher = indexUpsertPublisher;
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
                IsWound = true
            };

            channels.Add(channel);
            channelOwners.Add(new ChannelOwner()
            {
                OwnerId = cmd.UserId,
                Channel = channel,
                OwnedDate = DateTime.UtcNow,
                OwnerRole = ChannelOwnerRole.Admin
            });
            channelSubscriptions.Add(new ChannelSubscription()
            {
                Channel = channel,
                SubscriberId = cmd.UserId,
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

            var dtos = new ChannelDto[channels.Count];

            for (int i = 0; i < channels.Count; i++)
            {
                var channel = channels[i];

                var meta = await context.ChannelMetas
                    .AsNoTracking()
                    .FirstAsync(c => c
                        .ChannelId == channel.Id,
                        cancellationToken);

                dtos[i] = new ChannelDto(
                    channel.Id, channel.Name, "@" + channel.Slug,
                    channel.Description ?? "", channel.CreatedDate,
                    new PhotoDto(
                        new ImageVariants(
                            meta.IconBase,
                            meta.Small,
                            meta.Medium,
                            meta.Large),
                    channel.ChannelMeta.R,
                    channel.ChannelMeta.G,
                    channel.ChannelMeta.B), 1, 0, 1, 0, 0);

                await indexUpsertPublisher.Publish(
                    new SearchIndexUpsertMessage(nameof(Channel),
                        JsonSerializer.Serialize(new ChannelSearchIndex(channel, meta))), cancellationToken);
            }

            return Result<ChannelDto[]>.Success(201, dtos);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                Result<ChannelDto[]>.Failure(409, "User name already exists");

            throw;
        }
    }
}