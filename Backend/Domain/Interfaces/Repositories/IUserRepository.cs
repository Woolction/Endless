using Contracts.Rows;
using Domain.Entities;

namespace Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<UserSearchRow>> SearchUsersByName(string name, bool hasLastSearch, double lastScore, Guid lastId);
}