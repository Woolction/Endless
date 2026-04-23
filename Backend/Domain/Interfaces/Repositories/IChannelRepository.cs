using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch;
using Domain.Rows.Channels;
using Domain.Entities;

namespace Domain.Interfaces.Repositories;

public interface IChannelRepository
{
    Task<ChannelSearchRow> SearchChannelsByName(string name, ICollection<FieldValue> lastValues, CancellationToken cancellationToken);
    Task<DeleteResponse> DeleteSearchIndex(Guid channelId, CancellationToken cancellationToken);
    Task<IndexResponse> CreateSearchIndex(Channel channel, CancellationToken cancellationToken);
    Task<CreateIndexResponse> CreateMapping(CancellationToken cancellationToken);
}