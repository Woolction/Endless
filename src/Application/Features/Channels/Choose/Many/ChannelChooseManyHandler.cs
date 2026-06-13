using Application.Features.Rows.Contents;
using Application.Features.Channels.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Images;
using Application.Features.Images;
using Application.Interfaces.Db;
using MediatR;

namespace Application.Features.Channels.Choose.Many;

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
                new ImageDto(
                    new ImageVariantsDto(
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