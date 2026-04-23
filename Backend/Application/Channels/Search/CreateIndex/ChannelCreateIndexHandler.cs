using Application.Searchs;
using Domain.Interfaces.Repositories;
using Domain.Rows.Channels;
using Elastic.Clients.Elasticsearch;
using MediatR;

namespace Application.Channels.Search.CreateIndex;

public class ChannelCreateIndexHandler : IRequestHandler<ChannelCreateIndexCommand, Result<IndexCreatedDto>>
{
    private readonly IChannelRepository channelRepository;
    public ChannelCreateIndexHandler(IChannelRepository channelRepository)
    {
        this.channelRepository = channelRepository;
    }

    public async Task<Result<IndexCreatedDto>> Handle(ChannelCreateIndexCommand cmd, CancellationToken cancellationToken)
    {
        var response = await channelRepository.CreateMapping(cancellationToken);

        if (!response.IsValidResponse || !response.IsSuccess())
            return Result<IndexCreatedDto>.Failure(500, response.DebugInformation);

        return Result<IndexCreatedDto>.Success(201, new IndexCreatedDto(
            response.DebugInformation));
    }
}