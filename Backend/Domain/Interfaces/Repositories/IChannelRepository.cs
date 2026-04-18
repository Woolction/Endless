using Domain.Entities;
using Domain.Rows.Channels;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;

namespace Domain.Interfaces.Repositories;

public interface IChannelRepository
{
    Task<ChannelSearchRow> SearchChannelsByName(string name, ICollection<FieldValue> lastValues, CancellationToken cancellationToken);
    Task<DeleteResponse> DeleteSearchIndex(Guid channelId, CancellationToken cancellationToken);
    Task<IndexResponse> CreateSearchIndex(Channel channel, CancellationToken cancellationToken);
    Task<CreateIndexResponse> CreateMapping(CancellationToken cancellationToken);
}