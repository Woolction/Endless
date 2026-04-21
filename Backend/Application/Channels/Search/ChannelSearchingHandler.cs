using Application.Searchs;
using Application.Channels.Dtos;
using Domain.Interfaces.Repositories;
using MediatR;
using Domain.Rows.Channels;
using Microsoft.Extensions.Logging;
using Elastic.Clients.Elasticsearch;

namespace Application.Channels.Search;

public class ChannelSearchingHandler : IRequestHandler<ChannelSearchQuery, Result<SearchedChannelDto[]>>
{
    private readonly ILogger<ChannelSearchingHandler> logger;
    private readonly IChannelRepository channelRepository;

    public ChannelSearchingHandler(IChannelRepository channelRepository, ILogger<ChannelSearchingHandler> logger)
    {
        this.channelRepository = channelRepository;
        this.logger = logger;
    }

    public async Task<Result<SearchedChannelDto[]>> Handle(ChannelSearchQuery query, CancellationToken cancellationToken)
    {
        ICollection<FieldValue> lastValue = [];

        if (query.LastScore != null)
            lastValue.Add(FieldValue.Double(
                query.LastScore.Value));

        ChannelSearchRow result = await channelRepository.SearchChannelsByName(query.Name, lastValue, cancellationToken);

        SearchedChannelDto[] channelDtos = result.SearchedChannels.Select(c => new SearchedChannelDto(new ChannelDto(
            c.SearchedChannel.ChannelId, c.SearchedChannel.Name, c.SearchedChannel.Slug,
            c.SearchedChannel.Description, c.SearchedChannel.CreatedDate,
            c.SearchedChannel.AvatarPhotoUrl, 0, 0, 0, c.SearchedChannel.TotalLikes,
            c.SearchedChannel.TotalViews), c.Score)).ToArray();

        if (channelDtos.Length < 1)
            return Result<SearchedChannelDto[]>.Failure(404, $"Channel with name: {query.Name} not found");

        logger.LogInformation("Search returned Channels {Count} results for {Query}",
           channelDtos.Length, query.Name);

        return Result<SearchedChannelDto[]>.Success(200, channelDtos);
    }
}