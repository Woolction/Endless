using Domain.Entities;
using Domain.Rows.Users;

namespace Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<UserSearchRow>> SearchUsersByName(string name, bool hasLastSearch, double lastScore, Guid lastId, CancellationToken cancellationToken);
    Task CreateSearchIndex(User user);
}