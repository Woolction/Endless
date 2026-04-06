using Domain.Entities;

namespace Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByRefreshToken(string refreshToken);
    Task<User?> GetUserByEmail(string email);
    Task<bool> AnyUserByEmail(string email);
    void AddUser(User user);
    Task<int> SaveChangesAsync();
}