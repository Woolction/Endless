using Application.Searchs;
using Domain.Rows.Channels;
using Elastic.Clients.Elasticsearch;
using MediatR;

namespace Application.Channels.Search.CreateIndex;

public class ChannelCreateIndexHandler : IRequestHandler<ChannelCreateIndexCommand, Result<IndexCreatedDto>>
{
    private readonly ElasticsearchClient client;
    public ChannelCreateIndexHandler(ElasticsearchClient client)
    {
        this.client = client;
    }

    public async Task<Result<IndexCreatedDto>> Handle(ChannelCreateIndexCommand cmd, CancellationToken cancellationToken)
    {
        var hasIndex = await client.Indices.ExistsAsync("channels", cancellationToken);

        if (hasIndex.Exists)
            await client.Indices.DeleteAsync("channels", cancellationToken);

        var response = await client.Indices.CreateAsync("channels", c => c
            .Mappings(m => m
                .Properties<ChannelSearchIndex>(p => p
                    .Keyword(c => c.ChannelId)
                    .Text(c => c.Name)
                    .Text(c => c.Description)
                    .Date(c => c.CreatedDate)
        )), cancellationToken);

        if (!response.IsValidResponse || !response.IsSuccess())
            return Result<IndexCreatedDto>.Failure(500, response.DebugInformation);

        return Result<IndexCreatedDto>.Success(201, new IndexCreatedDto(
            response.DebugInformation));
    }
}