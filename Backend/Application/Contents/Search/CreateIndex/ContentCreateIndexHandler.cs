using Elastic.Clients.Elasticsearch;
using Domain.Rows.Contents;
using Application.Searchs;
using MediatR;

namespace Application.Contents.Search.CreateIndex;

public class ContentCreateIndexHandler : IRequestHandler<ContentCreateIndexCommand, Result<IndexCreatedDto>>
{
    private readonly ElasticsearchClient client;

    public ContentCreateIndexHandler(ElasticsearchClient client)
    {
        this.client = client;
    }

    public async Task<Result<IndexCreatedDto>> Handle(ContentCreateIndexCommand cmd, CancellationToken cancellationToken)
    {
        var hasIndex = await client.Indices.ExistsAsync("contents", cancellationToken);

        if (hasIndex.Exists)
            await client.Indices.DeleteAsync("contents", cancellationToken);

        var response = await client.Indices.CreateAsync("contents", a =>
            a.Mappings(m =>
                m.Properties<ContentSearchIndex>(p => p
                    .Keyword(c => c.ContentId)
                    .Keyword(c => c.CreatorId)
                    .Keyword(c => c.ChannelId)
                    .Text(c => c.Title)
                    .Text(c => c.Description)
                    .Date(c => c.CreatedDate)
        )));

        if (!response.IsValidResponse || !response.IsSuccess())
            return Result<IndexCreatedDto>.Failure(500, response.DebugInformation);

        return Result<IndexCreatedDto>.Success(201, new IndexCreatedDto(response.DebugInformation));
    }
}