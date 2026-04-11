using Application.Channels.Dtos;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.Channels.Choose.One;

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
                channel.AvatarPhotoUrl, channel.Subscribers.Count,
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