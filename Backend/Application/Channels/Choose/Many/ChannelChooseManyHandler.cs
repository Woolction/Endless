using Application.Channels.Dtos;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
                channel.AvatarPhotoUrl, channel.Subscribers.Count,
                channel.Contents.Count, channel.Owners.Count,
                channel.TotalLikes, channel.TotalViews))
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        logger.LogInformation("Returned {Count} Channels", channelDtos.Length);

        return Result<ChannelDto[]>.Success(200, channelDtos);
    }
}