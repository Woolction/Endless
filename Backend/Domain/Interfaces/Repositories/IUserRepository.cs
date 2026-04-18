using Domain.Entities;
using Domain.Rows.Users;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;

namespace Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<UserSearchRow> SearchUsersByName(string name, ICollection<FieldValue> lastValues, CancellationToken cancellationToken);
    Task<DeleteResponse> DeleteSearchIndex(Guid UserId, CancellationToken cancellationToken);
    Task<IndexResponse> CreateSearchIndex(User user, CancellationToken cancellationToken);
    Task<CreateIndexResponse> CreateMapping(CancellationToken cancellationToken);
}