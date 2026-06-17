using Application.Features.Channels.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Features.Images;
using Application.Interfaces.Db;
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
            .FirstOrDefaultAsync(cancellationToken);

        if (channelDto == null)
            return Result<ChannelDto>.Failure(404, "Channel not found");


        logger.LogInformation("Returned channel {ChannelId}",
            channelDto.Id);

        return Result<ChannelDto>.Success(200, channelDto);
    }
}