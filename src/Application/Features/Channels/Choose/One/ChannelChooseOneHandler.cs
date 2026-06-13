using Application.Features.Rows.Contents;
using Application.Features.Channels.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Images;
using Application.Interfaces.Db;
using Application.Features.Images;
using MediatR;

namespace Application.Features.Channels.Choose.One;

public class ChannelChooseOneHandler : IRequestHandler<ChannelChooseOneQuery, Result<ChannelDto>>
{
    private readonly ILogger<ChannelChooseOneHandler> logger;
    private readonly IAppDbContext context;

    public ChannelChooseOneHandler(IAppDbContext context, ILogger<ChannelChooseOneHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<Result<ChannelDto>> Handle(ChannelChooseOneQuery query, CancellationToken cancellationToken)
    {
        ChannelDto? channelDto = await context.Channels
            .AsNoTracking()
            .Where(channel => channel.Id == query.Id)
            .Select(channel => new ChannelDto(
                channel.Id, channel.Name, "@" + channel.Slug,
                channel.Description ?? "", channel.CreatedDate,
                new ImageDto(
                    new ImageVariants(
                        channel.ChannelMeta.IconBase,
                        channel.ChannelMeta.Small,
                        channel.ChannelMeta.Medium,
                        channel.ChannelMeta.Large),
                    channel.ChannelMeta.R,
                    channel.ChannelMeta.G,
                    channel.ChannelMeta.B),
                channel.Subscribers.Count,
                channel.Contents.Count, channel.Owners.Count,
                channel.TotalLikes, channel.TotalViews))
            .FirstOrDefaultAsync(cancellationToken);

        if (channelDto == null)
            return Result<ChannelDto>.Failure(404, "Channel not found");


        logger.LogInformation("Returned channel {ChannelId}",
            channelDto.Id);

        return Result<ChannelDto>.Success(200, channelDto);
    }
}