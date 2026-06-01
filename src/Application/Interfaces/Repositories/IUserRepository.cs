using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch;
using Application.Features.Rows.Users;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<UserSearchRow> SearchUsersByName(string name, ICollection<FieldValue> lastValues, CancellationToken cancellationToken);
    Task<DeleteResponse> DeleteSearchIndex(Guid userId, CancellationToken cancellationToken);
    Task<IndexResponse> CreateSearchIndex(UserSearchIndex index, CancellationToken cancellationToken);
    Task<CreateIndexResponse> CreateMapping(CancellationToken cancellationToken);
}