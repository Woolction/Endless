using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Channels.Dtos;
using Domain.Common.Interfaces.Db;
using MediatR;
using Domain.Rows.Contents;
using Application.Dtos;

namespace Application.Channels.Choose.Many;

public class ChannelChooseManyHandler : IRequestHandler<ChannelChooseManyQuery, Result<ChannelDto[]>>
{
    private readonly ILogger<ChannelChooseManyHandler> logger;
    private readonly IAppDbContext context;

    public ChannelChooseManyHandler(IAppDbContext context, ILogger<ChannelChooseManyHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<Result<ChannelDto[]>> Handle(ChannelChooseManyQuery query, CancellationToken cancellationToken)
    {
        ChannelDto[] channelDtos = await context.Channels
            .Select(channel => new ChannelDto(
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
                    channel.ChannelMeta.B), channel.Subscribers.Count,
                channel.Contents.Count, channel.Owners.Count,
                channel.TotalLikes, channel.TotalViews))
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        logger.LogInformation("Returned {Count} Channels", channelDtos.Length);

        return Result<ChannelDto[]>.Success(200, channelDtos);
    }
}