using Domain.Entities;
using Domain.Rows.Users;
using Elastic.Clients.Elasticsearch;

namespace Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<UserSearchRow> SearchUsersByName(string name, ICollection<FieldValue> lastValues, CancellationToken cancellationToken);
    Task CreateSearchIndex(User user);
}