using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch;
using Domain.Rows.Contents;
using Domain.Entities;

namespace Domain.Interfaces.Repositories;

public interface IContentRepository
{
    Task<ContentSearchRow> SearchContentsByName(string name, ICollection<FieldValue> lastValues, CancellationToken cancellationToken);
    Task<DeleteResponse> DeleteSearchIndex(Guid contentId, CancellationToken cancellationToken);
    Task<IndexResponse> CreateSearchIndex(Content content, CancellationToken cancellationToken);
    Task<CreateIndexResponse> CreateMapping(CancellationToken cancellationToken);
}