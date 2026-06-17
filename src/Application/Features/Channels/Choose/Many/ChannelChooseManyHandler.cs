using Application.Features.Channels.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
                        channel.ChannelMeta.Image.BaseUrl,
                        channel.ChannelMeta.Image.Variants
                            .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                            .ToList()),
                    channel.ChannelMeta.Image.R,
                    channel.ChannelMeta.Image.G,
                    channel.ChannelMeta.Image.B),
                channel.Subscribers.Count, channel.Contents.Count,
                channel.Owners.Count, channel.TotalLikes, channel.TotalViews))
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        logger.LogInformation("Returned {Count} Channels", channelDtos.Length);

        return Result<ChannelDto[]>.Success(200, channelDtos);
    }
}