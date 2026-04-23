using Elastic.Clients.Elasticsearch;
using MediatR;

namespace Application.Searchs.DeleteIndex;

public class DeleteIndexHandler : IRequestHandler<DeleteIndexCommand, Result<Null>>
{
    private readonly ElasticsearchClient client;

    public DeleteIndexHandler(ElasticsearchClient client)
    {
        this.client = client;
    }
    
    public async Task<Result<Null>> Handle(DeleteIndexCommand cmd, CancellationToken cancellationToken)
    {
        var hasIndex = await client.Indices.ExistsAsync(cmd.IndexName, cancellationToken);

        if (hasIndex.Exists)
        {
            await client.Indices.DeleteAsync(cmd.IndexName, cancellationToken);

            return Result<Null>.Success(204, new Null());
        }

        return Result<Null>.Failure(404, $"Index by name: {cmd.IndexName} not found");
    }
}