using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch;
using Application.Features.Rows.Channels;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IChannelRepository
{
    Task<ChannelSearchRow> SearchChannelsByName(string name, ICollection<FieldValue> lastValues, CancellationToken cancellationToken);
    Task<DeleteResponse> DeleteSearchIndex(Guid channelId, CancellationToken cancellationToken);
    Task<IndexResponse> CreateSearchIndex(ChannelSearchIndex index, CancellationToken cancellationToken);
    Task<CreateIndexResponse> CreateMapping(CancellationToken cancellationToken);
}