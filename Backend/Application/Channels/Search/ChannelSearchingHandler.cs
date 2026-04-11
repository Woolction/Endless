using Application.Searchs;
using Application.Channels.Dtos;
using Domain.Interfaces.Repositories;
using MediatR;
using Domain.Rows.Channels;
using Microsoft.Extensions.Logging;

namespace Application.Channels.Search;

public class ChannelSearchingHandler : IRequestHandler<ChannelSearchQuery, Result<ChannelSearchDto>>
{
    private readonly ILogger<ChannelSearchingHandler> logger;
    private readonly IChannelRepository channelRepository;

    public ChannelSearchingHandler(IChannelRepository channelRepository, ILogger<ChannelSearchingHandler> logger)
    {
        this.channelRepository = channelRepository;
        this.logger = logger;
    }

    public async Task<Result<ChannelSearchDto>> Handle(ChannelSearchQuery query, CancellationToken cancellationToken)
    {
        bool hasLastSearch = query.LastSearch != null;

        SearchDto searchDto = hasLastSearch == true ? query.LastSearch! : new SearchDto();

        IEnumerable<ChannelSearchRow> result = await channelRepository.SearchChannelsByName(query.Name, hasLastSearch, searchDto.LastScore, searchDto.LastId, cancellationToken);

        ChannelDto[] channelDtos = result.Select(c => new ChannelDto(
            c.Id, c.Name, c.Slug, c.Description, c.CreatedDate,
            c.AvatarPhotoUrl, c.SubscribersCount, c.ContentsCount,
            c.OwnersCount, c.TotalLikes, c.TotalViews
        )).ToArray();

        if (channelDtos.Length < 1)
            return Result<ChannelSearchDto>.Failure(404, $"Channel with name: {query.Name} not found");

        var last = result.Last();

        SearchDto lastSearch = new()
        {
            LastScore = last.Score,
            LastId = last.Id
        };

        logger.LogInformation("Search returned Channels {Count} results for {Query}",
           channelDtos.Length, query.Name);

        return Result<ChannelSearchDto>.Success(
            200, new ChannelSearchDto(channelDtos, lastSearch));
    }
}